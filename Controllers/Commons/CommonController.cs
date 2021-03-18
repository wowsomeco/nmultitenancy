using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MultiTenancy {
  [ApiController]
  [Route("[controller]")]
  [TypeFilter(typeof(TenantDomainFilterAttribute))]
  [TypeFilter(typeof(AuthFilterAttribute))]
  public abstract class CommonController<TEntityIdType, TEntity, TGet, TInsert, TUpdate, TDbContext> : GetControllerBase<TEntityIdType, TEntity, TGet, TDbContext>
  where TEntityIdType : IComparable
  where TEntity : class, IEntityHasId<TEntityIdType>
  where TGet : IGetDto<TEntity>, new()
  where TInsert : IInsertDto<TEntity>
  where TUpdate : IUpdateDto<TEntity>
  where TDbContext : TenantDbContext {
    public CommonController(TenantRepository<TEntity, TDbContext> repo) : base(repo) { }

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
  }
}