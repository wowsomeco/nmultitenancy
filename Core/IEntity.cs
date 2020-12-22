using System;

namespace MultiTenancy {
  public interface IEntity {
    int Id { get; init; }
    DateTimeOffset CreatedAt { get; init; }
    DateTimeOffset UpdatedAt { get; set; }
  }

  public interface ITenantScopedEntity : IEntity {
    int CompanyId { get; }
  }
}