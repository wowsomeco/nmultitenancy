using System.Threading.Tasks;

namespace MultiTenancy {
  public interface IAclProvider {
    Task<AclModel> GetAclModel(string role);
  }

  public class RedisAclProvider<T> : IAclProvider {
    private readonly IRedisClient _redis;
    private readonly ApplicationContext _appContext;

    public RedisAclProvider(IRedisClient redis, ApplicationContext ac) {
      _redis = redis;
      _appContext = ac;
    }

    public async Task<AclModel> GetAclModel(string role) {
      return await _redis.Get<AclModel>(_appContext.AppTenantId);
    }
  }
}