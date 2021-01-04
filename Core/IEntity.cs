using System;

namespace MultiTenancy {
  /// <summary>
  /// Whenever an entity implements this interface:
  /// - it will set the default value to 'now()' for both CreatedAt and UpdatedAt on inserted
  /// - it will automatically set UpdatedAt = DateTime.Now on updated
  /// </summary>
  public interface IEntity {
    DateTimeOffset CreatedAt { get; init; }
    DateTimeOffset UpdatedAt { get; set; }
  }

  /// <summary>
  /// Whenever an entity implements this interface,
  /// TenantDbContext will set the global filter so that it will only retrieve the entity that belong to the associated TenantId
  /// </summary>
  public interface ITenantScopedEntity : IEntity {
    string TenantId { get; set; }
  }
}