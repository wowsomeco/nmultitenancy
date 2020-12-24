using System;
using System.Collections.Generic;
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

  public static class StringExtensions {
    public static string ToUnderscore(this string str) {
      return str.Replace(" ", "_");
    }

    public static string ToUnderscoreLower(this string str) {
      return str.ToUnderscore().ToLower();
    }

    public static string Concat(params string[] str) {
      string concat = string.Empty;

      foreach (string s in str) {
        concat += s;
      }

      return concat;
    }


    public static string Capitalize(this string str, char separator = ' ', string replaceWith = " ") {
      string capitalized = "";
      string[] splits = str.Split(separator);

      for (int i = 0; i < splits.Length; i++) {
        String s = splits[i];
        capitalized += Concat(s[0].ToString().ToUpper(), s.Substring(1));
        if (i < splits.Length - 1) capitalized += replaceWith;
      }

      return capitalized;
    }

    /// <summary>
    /// Checks whether the string is empty
    /// </summary>
    public static bool IsEmpty(this string str) {
      return string.IsNullOrEmpty(str);
    }
  }

  public static class ListExtensions {
    public static List<TOut> Map<TIn, TOut>(this IEnumerable<TIn> list, Func<TIn, TOut> mapper) {
      List<TOut> newList = new List<TOut>();

      foreach (TIn itm in list)
        newList.Add(mapper(itm));

      return newList;
    }
  }

  public static class CommonExtensions {
    /// <summary>
    /// Try cast TIn to TOut.
    /// success callback will get triggered on success.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public static void TryCastTo<TIn, TOut>(this TIn t, Action<TOut> success)
    where TIn : class where TOut : class {
      if (t is TOut) success(t as TOut);
    }
  }
}