using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace MultiTenancy {
  public interface IRedisClient {
    Task<T> Get<T>(string key, bool addNX = false) where T : new();
    Task<bool> Set<T>(string key, T value);
    Task<bool> Delete(string key);
  }

  public class RedisClient : IRedisClient, IDisposable {
    private readonly Redis _impl;

    public RedisClient(AppConfig config, ILogHandler logger) {
      var key = "Redis:";
      var host = config[$"{key}Host"];
      var pwd = config[$"{key}Password"];
      var port = int.Parse(config[$"{key}Port"]);

      _impl = new Redis(host, port) {
        Password = pwd,
        Logger = logger
      };
    }

    public void Dispose() {
      _impl.Dispose();
    }

    public async Task<T> Get<T>(string key, bool addNX = false) where T : new() {
      return await Task.Run(() => {
        var byteVal = _impl.Get(key);

        T t = default(T);

        if (null != byteVal) {
          t = JsonSerializer.Deserialize<T>(byteVal);
        } else if (addNX) {
          t = new T();
          _impl.Set(key, JsonSerializer.Serialize(t).ToBytes());
        }

        return t;
      });
    }

    public async Task<bool> Set<T>(string key, T value) {
      return await Task.Run(() => {
        _impl.Set(key, JsonSerializer.Serialize(value));
        return true;
      });
    }

    public async Task<bool> Delete(string key) {
      return await Task.Run(() => {
        return _impl.Remove(key);
      });
    }
  }
}