using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MultiTenancy {
  /// <summary>
  /// Filter attribute that may be attached to ApiController to check whether any hostname is defined in the Request Header.  
  /// It throws 400 if no hostname can be found.  
  /// </summary>
  /// <example>
  /// <code>
  /// [TypeFilter(typeof(TenantDomainFilterAttribute))]
  /// [ApiController]
  /// public class CompanyController {}
  /// </code>  
  /// </example>
  public class TenantDomainFilterAttribute : ActionFilterAttribute {
    private readonly ApplicationContext _appContext;

    public TenantDomainFilterAttribute(ApplicationContext appContext) {
      _appContext = appContext;
    }

    public override void OnActionExecuting(ActionExecutingContext context) {
      if (_appContext.TenantHostname.IsEmpty()) {
        var key = _appContext.Config.TenantKey;
        throw new HttpException(HttpStatusCode.BadRequest, $"please provide tenant hostname in the header e.g {key} = hostname1");
      }
    }
  }
}