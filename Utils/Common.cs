using System;

namespace MultiTenancy {
  public delegate T Delegate<T>(T t);
  public delegate T Delegate<T, U>(U t);

  public static class Utils {
    public static bool If(bool condition, Action ifTrue = null, Action ifFalse = null) {
      if (condition) {
        ifTrue?.Invoke();
      } else {
        ifFalse?.Invoke();
      }

      return condition;
    }
  }
}