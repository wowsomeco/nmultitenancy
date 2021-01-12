using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MultiTenancy {
  using _ = Utils;

  /// <summary>
  /// WIP
  /// DO NOT USE THIS YET
  /// </summary>
  public class AclGuardAttribute : ActionFilterAttribute {
    public string Resource { get; set; }
    public string Action { get; set; }

    private readonly IAclProvider _aclProvider;

    public AclGuardAttribute(IAclProvider aclProvider) {
      _aclProvider = aclProvider;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
      // TODO: get the current user role from JwtService or something.
      AclModel model = await _aclProvider.GetAclModel("");

      _.IfNotNull(
        model,
        m => _.IfNotNull(
          model.GetAclList(Resource),
            al => _.IfNotNull(
              al.GetAction(Action),
              act => {
                if (!act.CompareStandard(Action))
                  throw HttpException.Unauthorized("Your role is not allowed to perform this action");
              }
            )
          )
      );

      await next();
    }
  }
}