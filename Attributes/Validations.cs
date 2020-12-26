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
}
