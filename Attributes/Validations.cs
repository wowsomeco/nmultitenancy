using System;
using System.ComponentModel.DataAnnotations;

namespace MultiTenancy {
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
  sealed public class NoWhitespaceAttribute : ValidationAttribute {
    public override bool IsValid(object value) {
      var str = value?.ToString();
      return !str.IsEmpty() && !str.Contains(' ');
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} can not contain spaces";
    }
  }

  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
  sealed public class YearRangeAttribute : ValidationAttribute {
    /// <summary>
    /// Min offset from the current year
    /// e.g. when set 100, it means cur year - 100
    /// </summary>
    /// <value></value>
    public int MinOffset { get; set; } = 100;
    /// <summary>
    /// Max offset from the current year.
    /// </summary>    
    public int MaxOffset { get; set; } = 0;

    public override bool IsValid(object value) {
      int curYear = DateTime.Now.Year;
      int y = int.Parse(value?.ToString());
      return y > (curYear - MinOffset) && y <= (curYear + MaxOffset);
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} is not a valid year";
    }
  }
}
