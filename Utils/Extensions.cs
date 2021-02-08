using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenancy {
  public static class MvcExtensions {
    /// <summary>
    /// Transforms validation error that gets thrown by MVC if one or more variables are invalid.
    /// </summary>
    /// <param name="c">The service collection</param>
    /// <param name="transformer">Callback that sends the list or error messages and returns the object as the format according to your liking</param>
    public static void UseErrValidationResponse(this IServiceCollection c, Func<IEnumerable<string>, object> transformer) {
      c.PostConfigure<ApiBehaviorOptions>(o => {
        o.InvalidModelStateResponseFactory = ctx => {
          var err = ctx.ModelState.Values.SelectMany(x => x.Errors)
            .Select(x => x.ErrorMessage);
          var resp = transformer(err);

          return new JsonResult(resp) {
            StatusCode = (int)HttpStatusCode.UnprocessableEntity
          };
        };
      });
    }
  }

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

    public static bool Is<T>(this string value) {
      if (string.IsNullOrEmpty(value)) return false;
      var conv = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));

      if (conv.CanConvertFrom(typeof(string))) {
        try {
          conv.ConvertFrom(value);
          return true;
        } catch {
        }
      }
      return false;
    }

    public static DateTime? ToDate(this string v, string format = "yyyy-MM-dd") {
      return DateTime.TryParseExact(v, format, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime parseDate) ? parseDate : null;
    }

    public static bool LetterOrDigitOnly(this string s) {
      foreach (var c in s) {
        if (!char.IsLetterOrDigit(c)) return false;
      }
      return true;
    }

    public static bool HasSpecialChar(this string s, string excludes = "") {
      string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,";
      foreach (var item in specialChar) {
        if (s.Contains(item) && !excludes.Contains(item)) return true;
      }

      return false;
    }
  }

  public static class ListExtensions {
    public delegate bool Iterator<T>(T item, int idx);
    public delegate void IteratorWithPointer<T>(T item, int idx, bool first, bool last);

    public static void LoopWithPointer<T>(this IList<T> l, IteratorWithPointer<T> iter) {
      for (int i = 0; i < l.Count; ++i) {
        iter(l[i], i, i == 0, i == l.Count - 1);
      }
    }

    public static void Loop<T>(this IList<T> l, Iterator<T> iter) {
      for (int i = 0; i < l.Count; ++i) {
        // breaks when iter returns false, 
        // otherwise iterate the next one
        if (!iter(l[i], i)) break;
      }
    }

    public static List<TOut> Map<TIn, TOut>(this IEnumerable<TIn> list, Func<TIn, TOut> mapper) {
      List<TOut> newList = new List<TOut>();

      foreach (TIn itm in list)
        newList.Add(mapper(itm));

      return newList;
    }

    public static List<T> Merge<T>(this List<T> list, params T[] items) {
      if (null != items) {
        foreach (T item in items) {
          list.Add(item);
        }
      }

      return list;
    }

    public static List<T> RemoveWhere<T>(this List<T> l, Predicate<T> p) {
      List<T> found = l.FindAll(p);
      if (found.Count > 0) l.RemoveAll(p);

      return found;
    }

    public static bool IsEmpty<T>(this IList<T> l) {
      return null == l || l.Count == 0;
    }

    public static void SafeAdd<T>(this List<T> l, T item) {
      if (l == null) l = new List<T>();
      l.Add(item);
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
      if (t is TOut o) success(o);
    }

    public static bool IsNullable<T>(this T obj) {
      if (obj == null) return true; // obvious
      Type type = typeof(T);
      if (!type.IsValueType) return true; // ref-type
      if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
      return false; // value-type
    }
  }
}