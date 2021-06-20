using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MultiTenancy {
  public abstract class CommonController<TEntityIdType, TEntity, TGet, TInsert, TUpdate, TDbContext> : GetControllerBase<TEntityIdType, TEntity, TGet, TDbContext>
  where TEntityIdType : IComparable
  where TEntity : class, IEntityHasId<TEntityIdType>
  where TDbContext : TenantDbContext {
    protected abstract TEntity OnInsert(TInsert model);
    protected abstract void OnUpdate(TEntity entity, TUpdate model);
    protected virtual InsertCondition<TEntity, TInsert> InsertCondition() => null;

    public CommonController(TenantRepository<TEntity, TDbContext> repo) : base(repo) { }

    /// <summary>
    /// Creates a new item
    /// </summary>                
    [HttpPost]
    public virtual async Task<IActionResult> Insert([FromBody] TInsert body) {
      var newEntity = OnInsert(body);

      var condition = InsertCondition();

      var result = await _repo.Insert(
        newEntity,
        condition == null ?
        null :
        e =>
          !Utils.If(
            _repo.Table.AlreadyExists(x => condition.RejectWhen(x, body)),
            () => throw condition.RejectReason(body)
          )
      );

      return Ok(MapFromEntity(result));
    }

    /// <summary>
    /// Updates an item by id.    
    /// </summary>                
    [HttpPut("{id}")]
    [ProducesValidationError]
    public virtual async Task<IActionResult> Update(TEntityIdType id, [FromBody] TUpdate body) {
      var result = await _repo.Update(
        t => {
          OnUpdate(t, body);
        },
        () => EntityName,
        x => x.Id.Equals(id)
      );

      return Ok(MapFromEntity(result));
    }
  }
}