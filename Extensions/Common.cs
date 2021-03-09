using System;
using System.IO;
using Microsoft.AspNetCore.Builder;

namespace MultiTenancy {
  public static class MiddlewareExtensions {
    public static void UseExceptionMiddleware(this IApplicationBuilder app) {
      app.UseMiddleware<ExceptionMiddleware>();
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

    public static DirectoryInfo CreateDirIfNotExist(string path) {
      if (path.IsEmpty()) throw new NullReferenceException("path cant be null");

      return !Directory.Exists(path) ? Directory.CreateDirectory(path) : new DirectoryInfo(path);
    }
  }
}