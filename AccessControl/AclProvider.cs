using System.Threading.Tasks;

namespace MultiTenancy {
  public interface IAclProvider {
    Task<AclModel> GetAclModel(string role);
    string ValidateAcl(AclList acl);
  }

  public abstract class RedisAclProvider : IAclProvider {
    private readonly IRedisClient _redis;
    private readonly ApplicationContext _appContext;

    public RedisAclProvider(IRedisClient redis, ApplicationContext ac) {
      _redis = redis;
      _appContext = ac;
    }

    public async Task<AclModel> GetAclModel(string role) {
      return await _redis.Get<AclModel>(_appContext.AppTenantId);
    }

    public abstract string ValidateAcl(AclList acl);
  }
}