using Microsoft.AspNetCore.Http;

namespace MultiTenancy {
  public class ApplicationContext {
    HttpContext _httpContext;

    public AppConfig Config { get; private set; }

    public ApplicationContext(IHttpContextAccessor httpContextAccessor, AppConfig config) {
      Config = config;
      _httpContext = httpContextAccessor.HttpContext;
    }

    public string TenantHostname {
      get {
        try {
          return _httpContext.Request.Headers[Config.TenantKey];
        } catch {
          throw HttpException.BadRequest("Can not find tenant hostname in the request header");
        }
      }
    }
  }
}