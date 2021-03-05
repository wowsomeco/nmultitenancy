using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace MultiTenancy {
  public class ApplicationContext {
    public AppConfig Config { get; private set; }

    private readonly IHttpContextAccessor _httpAccessor;

    public ApplicationContext(IHttpContextAccessor httpContextAccessor, AppConfig config) {
      Config = config;
      _httpAccessor = httpContextAccessor;
    }

    HttpContext HttpContext {
      get => _httpAccessor.HttpContext;
    }

    public string AuthToken {
      get => HttpContext.Request.Headers[HeaderNames.Authorization];
    }

    public string TenantHostname {
      get {
        string tenantHostname = HttpContext.Request.Headers[Config.TenantKey];
        return tenantHostname ?? throw HttpException.BadRequest("Can not find tenant hostname in the request header");
      }
    }

    public string AppTenantId {
      get => $"{Config.DbSchema}:{TenantHostname}";
    }

    public string TryGetTenantHostname() {
      string tenantHostname = HttpContext.Request.Headers[Config.TenantKey];
      return tenantHostname ?? "";
    }

    // TODO: should this be added as a method in JwtService instead ?
    public bool ValidateToken(out JwtSecurityToken jwtToken) {
      SecurityToken validatedToken = null;
      var securityKey = new SymmetricSecurityKey(Config.JwtSecretKey.ToBytes());
      var tokenHandler = new JwtSecurityTokenHandler();
      var authToken = AuthToken ?? throw HttpException.BadRequest("Can not find Auth token in the request header");

      try {
        tokenHandler.ValidateToken(
          authToken.Replace("Bearer ", ""),
          new TokenValidationParameters {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey
          },
          out validatedToken
        );

        jwtToken = validatedToken as JwtSecurityToken;
      } catch {
        throw HttpException.Unauthorized("Invalid Auth token");
      }

      return true;
    }
  }
}