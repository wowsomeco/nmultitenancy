using System;
using FluentValidation;

namespace MultiTenancy {
  public static class FluentValidationExt {
    public static IRuleBuilderOptions<T, int?> YearRange<T>(
      this IRuleBuilder<T, int?> ruleBuilder, int minYear = 1900, int maxOffset = 0) {
      int curYear = DateTime.Now.Year;
      int min = minYear;
      int max = curYear + maxOffset;

      return ruleBuilder
        .LessThanOrEqualTo(max)
        .GreaterThanOrEqualTo(min);
    }

    public static IRuleBuilderOptions<T, U> Optional<T, U>(this IRuleBuilderOptions<T, U> r, Func<T, U> condition) {
      return r.When(x => condition(x) != null);
    }

    public static IRuleBuilderOptions<T, string> DigitOnly<T>(this IRuleBuilder<T, string> ruleBuilder) {
      return ruleBuilder.Must(x => x.DigitOnly()).WithMessage("{PropertyName} only accepts digits");
    }

    public static IRuleBuilderOptions<T, string> NoWhitespace<T>(this IRuleBuilder<T, string> ruleBuilder) {
      return ruleBuilder.Must(x => !x.Contains(' ')).WithMessage("{PropertyName} cant contain any whitespaces");
    }

    public static IRuleBuilderOptions<T, string> NoSpecialCharacters<T>(this IRuleBuilder<T, string> ruleBuilder, string excludes = "-") {
      return ruleBuilder.Must(x => !x.HasSpecialChar(excludes)).WithMessage("{PropertyName} cant contain any special characters except " + $"'{excludes}'");
    }

    public static IRuleBuilderOptions<T, string> ValidDate<T>(this IRuleBuilder<T, string> ruleBuilder, string format = "yyyy-MM-dd") {
      var rule = ruleBuilder.Must(x => x.ToDate(format) != null)
        .WithMessage("{PropertyName} is not a valid date, accepted format = " + $"'{format}'");

      return rule;
    }

    public static IRuleBuilderOptions<T, string> ValidFutureDate<T>(this IRuleBuilder<T, string> ruleBuilder) {
      var rule = ruleBuilder
      .Must(x => {
        var dt = x.ToDate();
        if (null != dt) {
          return dt.Value.CompareTo(DateTime.Now) >= 0;
        }

        return false;
      })
      .WithMessage("{PropertyName} must be later than today's date");

      return rule;
    }

    public static IRuleBuilderOptions<T, string> FromTodayOnwards<T>(this IRuleBuilder<T, string> ruleBuilder) {
      var rule = ruleBuilder
      .Must(x => {
        var dt = x.ToDate();
        if (null != dt) {
          return dt.Value.DMYOnly().CompareTo(DateTime.Now.DMYOnly()) >= 0;
        }

        return false;
      })
      .WithMessage("{PropertyName} must be later than or equal today's date");

      return rule;
    }

    public static IRuleBuilderOptions<T, string> ValidMonthYear<T>(this IRuleBuilder<T, string> ruleBuilder) {
      var rule = ruleBuilder.Must(x => x.ToDateMY() != null)
        .WithMessage("{PropertyName} is not valid, accepted format = YYYY-MM");

      return rule;
    }

    public static IRuleBuilderOptions<T, U> Required<T, U>(this IRuleBuilder<T, U> ruleBuilder) where U : class {
      var errMsg = "{PropertyName} is required";

      return ruleBuilder.NotNull().WithMessage(errMsg).NotEmpty().WithMessage(errMsg);
    }
  }
}