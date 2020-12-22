using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MultiTenancy {
  public class ResponseMiddleware {
    private readonly RequestDelegate _next;

    public ResponseMiddleware(RequestDelegate next) {
      _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext) {
      string body = new StreamReader(httpContext.Request.Body).ReadToEnd();
      // it returns the body as is for now...
      // TODO: inject IConfig with response format...
      await httpContext.Response.WriteAsync(
        JsonSerializer.Serialize(body)
      );

      await _next(httpContext);
    }
  }
}