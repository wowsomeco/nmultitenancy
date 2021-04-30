using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MultiTenancy {
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
  sealed public class NoSpecialCharAttribute : CheckNullAttribute {
    public string Excludes { get; set; } = "";

    public override bool IsValid(object value) {
      var str = value?.ToString();

      if (str.IsEmpty()) return Nullable;

      return !str.HasSpecialChar(Excludes);
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} can not contain any special characters";
    }
  }

  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
  sealed public class LetterOrDigitAttribute : CheckNullAttribute {
    public override bool IsValid(object value) {
      var str = value?.ToString();

      if (str.IsEmpty()) return Nullable;

      return str.LetterOrDigitOnly();
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} can only contain letter or digit";
    }
  }

  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
  sealed public class NoWhitespaceAttribute : CheckNullAttribute {
    public override bool IsValid(object value) {
      if (base.IsValid(value)) return true;

      var str = value?.ToString();
      return !str.IsEmpty() && !str.Contains(' ');
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} can not contain spaces";
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

    public StringFilterAttribute(params string[] strings) : this(Comparer.Equals, strings) { }

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