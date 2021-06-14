using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MultiTenancy {
  [ApiController]
  [Route("[controller]")]
  public abstract class GetControllerBase<TEntityIdType, TEntity, TGet, TDbContext> : ControllerBase
  where TEntityIdType : IComparable
  where TEntity : class, IEntityHasId<TEntityIdType>
  where TDbContext : TenantDbContext {
    public Action OnGet { get; protected set; }

    protected readonly TenantRepository<TEntity, TDbContext> _repo;
    protected abstract TGet MapFromEntity(TEntity e);

    public GetControllerBase(TenantRepository<TEntity, TDbContext> repo) {
      _repo = repo;
    }

    protected string EntityName => typeof(TEntity).Name.Replace("Entity", "");

    protected virtual void PreGetAll(QueryModel query = null) {
      if (null != query) _repo.AfterFilter += q => query.ToQueryable(q);
    }

    /// <summary>
    /// Get all items
    /// </summary>            
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QueryModel query) {
      OnGet?.Invoke();
      PreGetAll(query);

      var result = await _repo.GetAll(
        e => MapFromEntity(e)
      );
      return Ok(result);
    }

    /// <summary>
    /// Get all with count
    /// </summary>            
    [HttpGet("with/count")]
    public async Task<IActionResult> GetAllWithCount([FromQuery] QueryModel query) {
      OnGet?.Invoke();
      PreGetAll();
      // get all the result first without the limit and offset
      List<TGet> result = await _repo.GetAll(
        e => MapFromEntity(e)
      );
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
    /// Get item by id.
    /// </summary>            
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(TEntityIdType id) {
      OnGet?.Invoke();

      var result = await _repo.GetOne(
        e => MapFromEntity(e),
        () => EntityName,
        x => x.Id.Equals(id)
      );
      return Ok(result);
    }

    /// <summary>
    /// Get Total of items in the database
    /// </summary>            
    [HttpGet("count")]
    [ProducesJson(typeof(CountResponse))]
    public async Task<IActionResult> Count() {
      return Ok(new CountResponse { Count = await _repo.Table.CountAsync() });
    }
  }
}