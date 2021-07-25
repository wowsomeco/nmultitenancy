using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace MultiTenancy {
  public class ApplicationContext {
    public AppConfig Config { get; private set; }

    private readonly IHttpContextAccessor _httpAccessor;

    public ApplicationContext(IHttpContextAccessor httpContextAccessor, AppConfig config) {
      Config = config;
      _httpAccessor = httpContextAccessor;
    }

    HttpContext HttpContext => _httpAccessor.HttpContext;

    public string AuthToken => HttpContext.Request.Headers[HeaderNames.Authorization];

    public string TenantHostname {
      get {
        string tenantHostname = HttpContext.Request.Headers[Config.TenantKey];
        return tenantHostname ?? throw HttpException.BadRequest("Can not find tenant hostname in the request header");
      }
    }

    public string AppTenantId => $"{Config.DbSchema}:{TenantHostname}";

    public string TryGetTenantHostname() {
      string tenantHostname = HttpContext.Request.Headers[Config.TenantKey];
      return tenantHostname ?? "";
    }
  }
}