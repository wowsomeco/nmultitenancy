using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MultiTenancy {
  [ApiController]
  [Route("[controller]")]
  public abstract class GetControllerBase<TEntityIdType, TEntity, TGet, TQuery, TDbContext> : ControllerBase
  where TEntityIdType : IComparable
  where TEntity : class, IEntityHasId<TEntityIdType>
  where TQuery : QueryModel
  where TDbContext : TenantDbContext {
    public Action OnGet { get; protected set; }
    public string EntityName => typeof(TEntity).Name.Replace("Entity", "");

    protected readonly TenantRepository<TEntity, TDbContext> _repo;
    protected abstract Task<List<TGet>> GetAllWhere(TQuery query);
    protected abstract TGet MapFromEntity(TEntity e);

    public GetControllerBase(TenantRepository<TEntity, TDbContext> repo) {
      _repo = repo;
    }

    /// <summary>
    /// Get all with count.
    /// </summary>            
    [HttpGet("with/count")]
    [TenantHeader]
    public async Task<IActionResult> GetAllWithCount([FromQuery] TQuery query) {
      // get all the result first without the limit and offset
      List<TGet> result = await GetAllWhere(query);
      int count = result.Count;
      // perform the limit and offset stuff here
      var filtered = result;
      if (query.Offset is int o) filtered = result.Skip(o).ToList();
      if (query.Limit is int l) filtered = filtered.Take(l).ToList();

      return Ok(
        new GetWithCountDto<TGet> {
          Data = filtered,
          Count = count
        }
      );
    }

    /// <summary>
    /// Get all
    /// </summary>            
    [HttpGet]
    [TenantHeader]
    public async Task<IActionResult> GetAll([FromQuery] QueryModel query) {
      _repo.AfterFilter += q => query.ToQueryable(q);

      var result = await _repo.GetAll(
        e => MapFromEntity(e)
      );

      return Ok(result);
    }

    /// <summary>
    /// Get by id.
    /// </summary>            
    [HttpGet("{id}")]
    [TenantHeader]
    public async Task<IActionResult> GetById(TEntityIdType id) {
      var result = await _repo.GetOne(
        e => MapFromEntity(e),
        () => EntityName,
        x => x.Id.Equals(id)
      );
      return Ok(result);
    }

    /// <summary>
    /// Get Count in the database
    /// </summary>            
    [HttpGet("count")]
    [TenantHeader]
    [ProducesJson(typeof(CountResponse))]
    public async Task<IActionResult> Count() {
      return Ok(new CountResponse { Count = await _repo.Table.CountAsync() });
    }
  }
}