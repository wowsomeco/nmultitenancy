using System;
using System.Linq;
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

    public static bool AlreadyExists<T>(this DbSet<T> dbset, Func<T, bool> predicate) where T : class {
      int count = dbset.Where(predicate).Count();
      return count > 0;
    }

    public static bool IfNotExists<T>(this DbSet<T> dbset, Func<T, bool> predicate) where T : class {
      return !dbset.AlreadyExists(predicate);
    }
  }

  public static class MiddlewareExtensions {
    public static void UseExceptionMiddleware(this IApplicationBuilder app) {
      app.UseMiddleware<ExceptionMiddleware>();
    }
  }
}