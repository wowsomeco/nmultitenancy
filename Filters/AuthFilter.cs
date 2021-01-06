using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MultiTenancy {
  /// <summary>
  /// WIP
  /// </summary>
  public class AuthFilterAttribute : ActionFilterAttribute {
    private readonly ApplicationContext _appContext;

    public AuthFilterAttribute(ApplicationContext appContext) {
      _appContext = appContext;
    }

    public override void OnActionExecuting(ActionExecutingContext context) {
      JwtSecurityToken jwtToken;
      if (_appContext.ValidateToken(out jwtToken)) {
        string username = jwtToken.Claims.First(x => x.Type == "username")?.Value;
        // TODO: validate user role...
      }
    }
  }
}