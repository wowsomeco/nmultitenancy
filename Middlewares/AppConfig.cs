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

    public AppConfig(IConfiguration config, Options options) {
      Config = config;
      AppOptions = options;
    }

    public IConfiguration Config { get; private set; }
    public Options AppOptions { get; private set; }

    public string DbSchema {
      get {
        return Config[$"{_appSettingsPrefix}:Schema"];
      }
    }

    public string TenantKey {
      get {
        return Config[$"{_appSettingsPrefix}:TenantKey"];
      }
    }

    public string JwtSecretKey {
      get {
        return Config[$"{_jwtPrefix}:Secret"];
      }
    }
  }
}