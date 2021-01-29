using System;
using System.Collections.Generic;
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
  sealed public class MoreThanAttribute : ValidationAttribute {
    public int MinValue { get; set; } = 0;
    public bool Nullable { get; set; } = false;

    public override bool IsValid(object value) {
      if (value == null && Nullable) return true;

      int v;
      if (int.TryParse(value?.ToString(), out v)) {
        return v > MinValue;
      }

      return false;
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} needs to be more than {MinValue}";
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

      int y;
      if (int.TryParse(value?.ToString(), out y)) {
        return y > (curYear - MinOffset) && y <= (curYear + MaxOffset);
      }

      return false;
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} is not a valid year";
    }
  }

  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
  public class StringFilterAttribute : ValidationAttribute {
    public enum Comparer {
      Equals,
      StartsWith,
      EndsWith,
      Contains,
    }

    private List<string> _strings;
    private readonly Comparer _comparer;
    private readonly Dictionary<Comparer, Func<string, bool>> _handlers;

    public StringFilterAttribute(Comparer c = Comparer.Equals, params string[] strings) {
      _comparer = c;
      _strings = strings == null ? new List<string>() : new List<string>(strings);

      _handlers = new Dictionary<Comparer, Func<string, bool>> {
        {
          Comparer.Equals,
          str => _strings.Exists(x => x.CompareStandard(str))
        },
        {
          Comparer.StartsWith,
          str => _strings.Exists(x => x.StartsWith(str))
        },
        {
          Comparer.EndsWith,
          str => _strings.Exists(x => x.EndsWith(str))
        },
        {
          Comparer.Contains,
          str => _strings.Contains(str)
        },
      };
    }

    public override bool IsValid(object value) {
      var str = value?.ToString();
      return str.IsEmpty() ? false : _handlers[_comparer](str);
    }

    public override string FormatErrorMessage(string name) {
      var flatten = _strings.Flatten(',');
      return ErrorMessage ?? $"{name} must be either {flatten}";
    }
  }
}
