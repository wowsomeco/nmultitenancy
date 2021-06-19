using System;

namespace MultiTenancy {
  public static class DateTimeExtensions {
    public static string ToDateTz(this DateTimeOffset dt) {
      return dt.ToString("dd-MM-yyyy H:mm:ss");
    }

    public static string ToMYString(this DateTime dt) {
      string dtStr = dt.ToDateString();
      return dtStr.Substring(0, dtStr.Length - 3);
    }

    public static string ToMYString(this DateTime? dt) {
      if (null == dt) return null;

      return dt.Value.ToMYString();
    }

    public static string ToDateString(this DateTime dt) {
      return dt.ToString("yyyy-MM-dd");
    }

    public static string ToDateString(this DateTime? dt) {
      return dt == null ? null : dt?.ToDateString();
    }

    public static string ToDate(this DateTimeOffset d) {
      return d.ToString("dd-MM-yyyy");
    }

    public static string ToDatetime(this DateTimeOffset d) {
      return d.ToString("dd-MM-yyyy H:mm:ss");
    }

    public static DateTime DMYOnly(this DateTime dt) {
      return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, dt.Kind);
    }
  }
}