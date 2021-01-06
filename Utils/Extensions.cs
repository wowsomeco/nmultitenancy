using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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

    public static string Standardize(this string str) {
      return str.Trim().ToLower();
    }

    public static bool CompareStandard(this string str, string other) {
      return str.Standardize() == other.Standardize();
    }

    public static string Ellipsis(this string str, int maxLength = 10) {
      if (str.IsEmpty() || str.Length < maxLength) return str;

      string sub = str.Substring(0, maxLength);
      return sub + "...";
    }

    public static string LastSplit(this string str, char separator = '/') {
      return str.Split(separator).Last();
    }

    public static string FirstSplit(this string str, char separator = '/') {
      return str.Split(separator).First();
    }

    /// <summary>
    /// Checks whether the string is empty
    /// </summary>
    public static bool IsEmpty(this string str) {
      return string.IsNullOrEmpty(str);
    }

    public static byte[] ToBytes(this string str) {
      return Encoding.ASCII.GetBytes(str);
    }

    public static string FileExtension(this string str) {
      return Path.GetExtension(str);
    }

    public static string Flatten(this IList<string> strs, char separator = ' ') {
      string s = string.Empty;
      strs.LoopWithPointer((str, idx, first, last) => {
        s += str;
        if (!last) s += separator;
      });

      return s;
    }
  }

  public static class ListExtensions {
    public delegate void IteratorWithPointer<T>(T item, int idx, bool first, bool last);

    public static void LoopWithPointer<T>(this IList<T> l, IteratorWithPointer<T> iter) {
      for (int i = 0; i < l.Count; ++i) {
        iter(l[i], i, i == 0, i == l.Count - 1);
      }
    }

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