using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MultiTenancy {
  [ApiController]
  [Route("[controller]")]
  [TypeFilter(typeof(TenantDomainFilterAttribute))]
  [TypeFilter(typeof(AuthFilterAttribute))]
  public abstract class GetControllerBase<TEntityIdType, TEntity, TGet, TDbContext> : ControllerBase
  where TEntityIdType : IComparable
  where TEntity : class, IEntityHasId<TEntityIdType>
  where TGet : IGetDto<TEntity>, new()
  where TDbContext : TenantDbContext {
    protected readonly TenantRepository<TEntity, TDbContext> _repo;

    public GetControllerBase(TenantRepository<TEntity, TDbContext> repo) {
      _repo = repo;
    }

    protected TGet GetDto(TEntity e) {
      TGet dto = new TGet();
      dto.FromEntity(e);
      return dto;
    }

    protected string EntityName => typeof(TEntity).Name.Replace("Entity", "");

    /// <summary>
    /// Get all items
    /// </summary>            
    [HttpGet]
    [TenantHeader]
    public async Task<IActionResult> GetAll([FromQuery] QueryModel query) {
      _repo.AfterFilter += q => query.ToQueryable(q);

      var result = await _repo.GetAll(
        e => GetDto(e)
      );
      return Ok(result);
    }

    /// <summary>
    /// Get item by id.
    /// </summary>            
    [HttpGet("{id}")]
    [TenantHeader]
    public async Task<IActionResult> GetById(TEntityIdType id) {
      var result = await _repo.GetOne(
        e => GetDto(e),
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
    [TenantHeader]
    public async Task<IActionResult> Count() {
      return Ok(new CountResponse { Count = await _repo.Table.CountAsync() });
    }
  }
}