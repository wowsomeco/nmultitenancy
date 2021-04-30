using System;
using System.ComponentModel.DataAnnotations;

namespace MultiTenancy {
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
  public class CheckNullAttribute : ValidationAttribute {
    public bool Nullable { get; set; } = true;

    public override bool IsValid(object value) {
      // valid when Nullable and value is null
      if (Nullable && value == null) {
        return true;
      }
      // otherwise perform validation for value cant be null
      return value != null;
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} can not be null";
    }
  }
}