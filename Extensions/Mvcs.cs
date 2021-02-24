using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenancy {
  public static class MvcExtensions {
    /// <summary>
    /// Transforms validation error that gets thrown by MVC if one or more variables are invalid.
    /// </summary>
    /// <param name="c">The service collection</param>
    /// <param name="transformer">Callback that sends the list or error messages and returns the object as the format according to your liking</param>
    public static void UseErrValidationResponse(this IServiceCollection c, Func<IEnumerable<string>, object> transformer) {
      c.PostConfigure<ApiBehaviorOptions>(o => {
        o.InvalidModelStateResponseFactory = ctx => {
          var err = ctx.ModelState.Values.SelectMany(x => x.Errors)
            .Select(x => x.ErrorMessage);
          var resp = transformer(err);

          return new JsonResult(resp) {
            StatusCode = (int)HttpStatusCode.UnprocessableEntity
          };
        };
      });
    }
  }
}