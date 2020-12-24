using System;

namespace MultiTenancy {
  public interface IEntity {
    DateTimeOffset CreatedAt { get; init; }
    DateTimeOffset UpdatedAt { get; set; }
  }

  public interface ITenantScopedEntity : IEntity {
    string TenantId { get; }
  }
}