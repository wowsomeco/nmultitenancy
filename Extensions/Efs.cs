using System;
using System.Linq;
using System.Linq.Expressions;
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

    public static void GetEntity<T>(this ModelBuilder b, params Action<EntityTypeBuilder<T>>[] callbacks) where T : class {
      var t = b.Entity<T>();
      foreach (var c in callbacks) {
        c(t);
      }
    }

    public static void UniqueConstraints<T>(this EntityTypeBuilder<T> typeBuilder, Expression<Func<T, object>> cb) where T : class {
      typeBuilder.HasIndex(cb).IsUnique();
    }

    public static bool AlreadyExists<T>(this DbSet<T> dbset, Func<T, bool> predicate) where T : class {
      int count = dbset.Where(predicate).Count();
      return count > 0;
    }

    public static bool IfNotExists<T>(this DbSet<T> dbset, Func<T, bool> predicate) where T : class {
      return !dbset.AlreadyExists(predicate);
    }
  }
}