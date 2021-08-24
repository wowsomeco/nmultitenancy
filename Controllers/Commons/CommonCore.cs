using System.Collections.Generic;

namespace MultiTenancy {
  public interface IEntityHasId<TType> : IEntity {
    TType Id { get; }
  }

  public class GetWithCountDto<T> {
    public List<T> Data { get; set; }
    public int Count { get; set; }
  }
}