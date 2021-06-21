using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MultiTenancy {
  public class AuthFilterAttribute : ActionFilterAttribute {
    private readonly ApplicationContext _appContext;
    private JwtSecurityToken _jwtToken = null;

    public AuthFilterAttribute(ApplicationContext appContext) {
      _appContext = appContext;
    }

    public override void OnActionExecuting(ActionExecutingContext context) {
      if (_appContext.ValidateToken(out _jwtToken)) {
        OnAuthenticated(context);
      }
    }

    public bool TryGetClaim(string key, out string value) {
      var c = _jwtToken.Claims.First(x => x.Type == key);
      value = c?.Value;
      return c != null;
    }

    public virtual void OnAuthenticated(ActionExecutingContext context) { }
  }

  public class ClaimFilterAttribute : AuthFilterAttribute {
    private string[] _filters;
    private readonly ILogHandler _logger;

    public ClaimFilterAttribute(ApplicationContext appContext, ILogHandler logger, string[] filters) : base(appContext) {
      _filters = filters;
      _logger = logger;
    }

    public override void OnAuthenticated(ActionExecutingContext context) {
      foreach (var f in _filters) {
        var strs = f.Split(',');
        if (strs.Length != 2) continue;
        var k = strs[0].Trim();
        var v = strs[1].Trim();

        string claimValue;
        if (TryGetClaim(k, out claimValue)) {
          // skip since it means that claim has key with any value
          // e.g. 'username,*' means as long as claim has a key 'username' then it's valid.
          if (v == "*") continue;

          _logger.LogDebug($"{k},{claimValue}");

          if (!v.CompareStandard(claimValue)) {
            throw HttpException.Unauthorized("Unauthorized");
          }
        }
      }
    }
  }
}