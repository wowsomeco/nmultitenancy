using System;
using System.ComponentModel.DataAnnotations;

namespace MultiTenancy {
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
  sealed public class ValidDateAttribute : ValidationAttribute {
    public string Format { get; set; } = "yyyy-MM-dd";

    public override bool IsValid(object value) {
      var str = value?.ToString();

      if (!str.IsEmpty()) {
        return str.ToDate(Format) != null;
      }

      return false;
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} is not a valid date, format should be {Format}";
    }
  }

  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
  sealed public class FromTodayOnwardsAttribute : ValidationAttribute {
    public string Format { get; set; } = "yyyy-MM-dd";

    public override bool IsValid(object value) {
      var str = value?.ToString();

      if (!str.IsEmpty()) {
        return str.IsTodayOnwards();
      }

      return false;
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} must be later than today's date";
    }
  }
}