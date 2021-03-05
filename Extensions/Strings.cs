using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MultiTenancy {
  public static class StringExtensions {
    public static string ToDash(this string str) {
      return str.Replace(" ", "-");
    }

    public static string ToDashLower(this string str) {
      return str.ToDash().ToLower();
    }

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
        if (!s.IsEmpty() && s.Contains(item) && !excludes.Contains(item)) return true;
      }

      return false;
    }

    public static DateTime? ToDateMY(this string str) {
      return str.IsEmpty() ? null : (str + "-01").ToDate();
    }
  }
}