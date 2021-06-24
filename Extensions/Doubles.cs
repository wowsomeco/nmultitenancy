namespace MultiTenancy {
  public static class DoubleExtensions {
    public static bool IsBetween(this double d, double min, double max) {
      return d >= min && d <= max;
    }
  }
}