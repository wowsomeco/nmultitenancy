using System;
using System.ComponentModel.DataAnnotations;

namespace MultiTenancy {
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
  public class CheckNullAttribute : ValidationAttribute {
    public bool Nullable { get; set; } = true;

    public override bool IsValid(object value) {
      // if it's nullable, it's always valid
      if (Nullable) {
        return true;
      }
      // otherwise perform validation for valu cant be null
      return value != null;
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} can not be null";
    }
  }
}