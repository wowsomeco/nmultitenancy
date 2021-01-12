using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MultiTenancy {
  public static class Utils {
    public static bool If(bool condition, Action ifTrue = null, Action ifFalse = null) {
      if (condition) {
        ifTrue?.Invoke();
      } else {
        ifFalse?.Invoke();
      }

      return condition;
    }

    public static T IfNotNull<T>(T condition, Action<T> ifTrue = null, Action ifFalse = null) where T : class {
      if (condition != null) {
        ifTrue?.Invoke(condition);
      } else {
        ifFalse?.Invoke();
      }

      return condition;
    }

    public static IEnumerable<ValidationResult> DoValidate<T>(this T t, params Func<T, string>[] validations) where T : class {
      var results = new List<ValidationResult>();
      foreach (var v in validations) {
        string err = v(t);
        if (!err.IsEmpty()) results.Add(new ValidationResult(err));
      }
      return results;
    }
  }
}