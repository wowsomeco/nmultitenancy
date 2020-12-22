using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MultiTenancy {
  public class HttpException : Exception {
    private readonly HttpStatusCode _code;

    public int StatusCode {
      get { return (int)_code; }
    }

    public HttpException(HttpStatusCode code, string message) : base(message) {
      _code = code;
    }

    public HttpException(HttpStatusCode code) : this(code, code.ToString()) { }
  }

  public class ExceptionMiddleware {
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next) {
      // TODO: Inject ILogger
      _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext) {
      try {
        await _next(httpContext);
      } catch (Exception ex) {
        // TODO: Log exception message
        await HandleExceptionAsync(httpContext, ex);
      }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception) {
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

      return context.Response.WriteAsync(
        JsonSerializer.Serialize(new {
          StatusCode = context.Response.StatusCode,
          Message = exception.Message
        })
      );
    }
  }
}