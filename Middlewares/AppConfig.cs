using System;
using Microsoft.Extensions.Configuration;

namespace MultiTenancy {
  public interface IConfig { }

  public class AppConfig : IConfig {
    public class Options {
      public Func<string, string> ErrFormat { get; set; }
    }

    private readonly string _appSettingsPrefix = "Multitenancy";
    private readonly string _jwtPrefix = "Jwt";

    public string this[string key] {
      get => Config[key] ?? throw new NullReferenceException($"{key} does not exist in appsettings");
    }

    public AppConfig(IConfiguration config, Options options) {
      Config = config;
      AppOptions = options;
    }

    public IConfiguration Config { get; private set; }
    public Options AppOptions { get; private set; }

    public string DbSchema {
      get => Config[$"{_appSettingsPrefix}:Schema"];
    }

    public string TenantKey {
      get => Config[$"{_appSettingsPrefix}:TenantKey"];
    }

    public string JwtSecretKey {
      get => Config[$"{_jwtPrefix}:Secret"];
    }
  }
}