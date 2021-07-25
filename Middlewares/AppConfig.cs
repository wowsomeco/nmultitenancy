using System;
using Microsoft.Extensions.Configuration;

namespace MultiTenancy {
  public interface IConfig { }

  public class AppConfig : IConfig {
    public class Options {
      public Func<string, string> ErrFormat { get; set; }
    }

    public string this[string key] {
      get => Config[key];
    }
    public string DbSchema => Config[$"{_appSettingsPrefix}:Schema"];
    public string TenantKey => Config[$"{_appSettingsPrefix}:TenantKey"];

    private readonly string _appSettingsPrefix = "Multitenancy";

    public AppConfig(IConfiguration config, Options options) {
      Config = config;
      AppOptions = options;
    }

    public IConfiguration Config { get; private set; }
    public Options AppOptions { get; private set; }
  }
}