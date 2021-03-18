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
}