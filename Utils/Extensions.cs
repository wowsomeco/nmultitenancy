using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MultiTenancy {
  public static class EFExtensions {
    public static ModelBuilder EntitiesOfType<T>(this ModelBuilder modelBuilder, Action<EntityTypeBuilder> buildAction) where T : class {
      return modelBuilder.EntitiesOfType(typeof(T), buildAction);
    }

    public static ModelBuilder EntitiesOfType(this ModelBuilder modelBuilder, Type type, Action<EntityTypeBuilder> buildAction) {
      foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        if (type.IsAssignableFrom(entityType.ClrType))
          buildAction(modelBuilder.Entity(entityType.ClrType));

      return modelBuilder;
    }
  }

  public static class MiddlewareExtensions {
    public static void UseExceptionMiddleware(this IApplicationBuilder app) {
      app.UseMiddleware<ExceptionMiddleware>();
    }
  }
}