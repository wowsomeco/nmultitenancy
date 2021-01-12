using System;

namespace MultiTenancy {
  public interface ILogHandler {
    void LogException(Exception e);
    void LogDebug(object msg);
  }
}