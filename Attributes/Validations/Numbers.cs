using System;

namespace MultiTenancy {
  sealed public class DoubleMoreThanAttribute : CheckNullAttribute {
    public double MinValue { get; set; } = 0.0;

    public override bool IsValid(object value) {
      if (!base.IsValid(value)) return false;

      string strValue = value?.ToString();

      if (!strValue.IsEmpty()) {
        double v;
        if (double.TryParse(strValue, out v)) {
          return v > MinValue;
        }
      }

      return false;
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} needs to be more than {MinValue}";
    }
  }

  sealed public class IntMoreThanAttribute : CheckNullAttribute {
    public int MinValue { get; set; } = 0;

    public override bool IsValid(object value) {
      if (!base.IsValid(value)) return false;

      string strValue = value?.ToString();

      if (!strValue.IsEmpty()) {
        int v;
        if (int.TryParse(strValue, out v)) {
          return v > MinValue;
        }
      }

      return false;
    }

    public override string FormatErrorMessage(string name) {
      return ErrorMessage ?? $"{name} needs to be more than {MinValue}";
    }
  }

  sealed public class YearRangeAttribute : CheckNullAttribute {
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
      if (!base.IsValid(value)) return false;

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
}