using System;
using System.Net;
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

    public static HttpException AlreadyExists(string msg) {
      return new HttpException(HttpStatusCode.Conflict, $"{msg} already exists in the database");
    }

    public static HttpException NotExists(string msg) {
      return new HttpException(HttpStatusCode.NotFound, $"{msg} does not exist in the database");
    }

    public static HttpException BadRequest(string msg) {
      return new HttpException(HttpStatusCode.BadRequest, msg);
    }

    public static HttpException Unauthorized(string msg) {
      return new HttpException(HttpStatusCode.Unauthorized, msg);
    }
  }

  /// <summary>
  /// Catches any Exception and returns it as a json response body.
  /// </summary>
  public class ExceptionMiddleware {
    private readonly RequestDelegate _next;
    private readonly AppConfig _appConfig;
    private readonly ILogHandler _logger;

    public ExceptionMiddleware(RequestDelegate next, AppConfig appConfig, ILogHandler logger) {
      // TODO: Inject ILogger
      _next = next;
      _appConfig = appConfig;
      _logger = logger;
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
      _logger.LogException(exception);

      string msg = exception.Message;

      context.Response.ContentType = "application/json";
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

      // TODO: might need to create a factory to handle the exceptions
      // e.g. using IExceptionHandler singleton...
      HttpException httpException = exception as HttpException;
      if (null != httpException) {
        context.Response.StatusCode = httpException.StatusCode;
      }

      return context.Response.WriteAsync(
        _appConfig.AppOptions.ErrFormat(msg)
      );
    }
  }
}