using Microsoft.Extensions.Configuration;

namespace MultiTenancy {
  public interface IConfig { }

  public class AppConfig : IConfig {
    public class Options {
      public Delegate<string> ErrFormat { get; set; }
    }

    private readonly string _appSettingsPrefix = "Multitenancy";
    private readonly string _jwtPrefix = "Jwt";
    private readonly IConfiguration _config;

    public AppConfig(IConfiguration config, Options options) {
      _config = config;
      AppOptions = options;
    }

    public Options AppOptions { get; private set; }

    public string DbSchema {
      get {
        return _config[$"{_appSettingsPrefix}:Schema"];
      }
    }

    public string TenantKey {
      get {
        return _config[$"{_appSettingsPrefix}:TenantKey"];
      }
    }

    public string JwtSecretKey {
      get {
        return _config[$"{_jwtPrefix}:Secret"];
      }
    }
  }
}