using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace MultiTenancy {
  public class CacheModel {
    public Dictionary<string, byte[]> Data { get; init; } = new Dictionary<string, byte[]>();

    public T GetValue<T>(string key) where T : new() {
      if (Data.ContainsKey(key)) {
        return JsonSerializer.Deserialize<T>(Data[key]);
      }

      var t = new T();
      Data[key] = JsonSerializer.Serialize(t).ToBytes();

      return t;
    }

    public void SetValue<T>(string key, T value) {
      Data[key] = JsonSerializer.Serialize(value).ToBytes();
    }
  }

  // TODO: unit test this
  public class LocalCacheService {
    IMemoryCache _cache;
    string _tenantId;

    public LocalCacheService(IMemoryCache cache, ApplicationContext ctx) {
      _cache = cache;
      _tenantId = ctx.TenantHostname;
    }

    public TItem GetValue<TItem>(string key) where TItem : new() {
      return GetTenantCache().GetValue<TItem>(key);
    }

    public bool TryGetValue<TItem>(string key, out TItem item) where TItem : class, new() {
      item = null;

      CacheModel cm = GetTenantCache();
      if (cm.Data.ContainsKey(key)) {
        item = cm.GetValue<TItem>(key);
      }

      return item != null;
    }

    public void SetValue<TItem>(string key, TItem item) {
      GetTenantCache().SetValue(key, item);
    }

    CacheModel GetTenantCache() {
      CacheModel cm = null;
      if (!_cache.TryGetValue<CacheModel>(_tenantId, out cm)) {
        cm = new CacheModel();
        _cache.Set(_tenantId, cm);
      }

      return cm;
    }
  }
}