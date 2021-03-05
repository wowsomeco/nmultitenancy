using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MultiTenancy {
  public interface IEntityHasId<TType> : IEntity {
    TType Id { get; }
  }

  public interface IGetDto<T> {
    void FromEntity(T e);
  }

  public interface IUpdateDto<T> {
    void OnUpdate(T entity);
  }

  public interface IInsertDto<T> {
    T ToEntity();
  }

  [ApiController]
  [Route("[controller]")]
  [TypeFilter(typeof(TenantDomainFilterAttribute))]
  [TypeFilter(typeof(AuthFilterAttribute))]
  public abstract class CommonController<TEntityIdType, TEntity, TGet, TInsert, TUpdate, TDbContext> : ControllerBase
  where TEntityIdType : IComparable
  where TEntity : class, IEntityHasId<TEntityIdType>
  where TGet : IGetDto<TEntity>, new()
  where TInsert : IInsertDto<TEntity>
  where TUpdate : IUpdateDto<TEntity>
  where TDbContext : TenantDbContext {
    private readonly TenantRepository<TEntity, TDbContext> _repo;

    public CommonController(TenantRepository<TEntity, TDbContext> repo) {
      _repo = repo;
    }

    TGet GetDto(TEntity e) {
      TGet dto = new TGet();
      dto.FromEntity(e);
      return dto;
    }

    string EntityName {
      get => typeof(TEntity).Name.Replace("Entity", "");
    }

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
    /// Creates a new item
    /// </summary>                
    [HttpPost]
    [TenantHeader]
    public async Task<IActionResult> Insert([FromBody] TInsert body) {
      var newEntity = body.ToEntity();
      var result = await _repo.Insert(
        newEntity,
        e =>
          !Utils.If(
            _repo.Table.AlreadyExists(x => x.Id.Equals(newEntity.Id)),
            () => throw HttpException.AlreadyExists($"{newEntity.Id}")
          )
      );

      return Ok(GetDto(result));
    }

    /// <summary>
    /// Updates an item by id.    
    /// </summary>                
    [HttpPut("{id}")]
    [ProducesValidationError]
    [TenantHeader]
    public async Task<IActionResult> Update(TEntityIdType id, [FromBody] TUpdate body) {
      var result = await _repo.Update(
        t => {
          body.OnUpdate(t);
        },
        () => EntityName,
        x => x.Id.Equals(id)
      );

      return Ok(GetDto(result));
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