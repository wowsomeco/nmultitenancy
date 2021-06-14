using System.Collections.Generic;

namespace MultiTenancy {
  public interface IEntityHasId<TType> : IEntity {
    TType Id { get; }
  }

  public class GetWithCountDto<T> {
    public List<T> Data { get; set; }
    public int Count { get; set; }
  }

  public class InsertCondition<TEntity, TInsert> {
    public delegate bool Reject(TEntity e, TInsert model);
    public delegate HttpException Reason(TInsert model);

    public Reject RejectWhen { get; set; }
    public Reason RejectReason { get; set; }
  }
}