using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MultiTenancy {
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
  public class ProducesValidationErrorAttribute : ProducesResponseTypeAttribute {
    public ProducesValidationErrorAttribute(Type type = null) : base(type ?? typeof(IErrorResponse), (int)HttpStatusCode.UnprocessableEntity) {
      Type = type;
    }
  }

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
  public class ProducesJsonAttribute : ProducesAttribute {
    public ProducesJsonAttribute(Type type) : base(MediaTypeNames.Application.Json) {
      Type = type;
    }
  }

  /// <summary>
  /// Tells Swagger UI to add Tenant header param.  
  /// </summary>
  /// <example>
  /// <code>
  /// [TenantHeader]
  /// public async Task Login([FromBody] LoginDto model) {
  ///   
  /// }
  /// </code>
  /// </example>
  public class TenantHeaderAttribute : Attribute {
    public string Description { get; set; } = "The Tenant Id";
  }

  /// <summary>
  /// Handles TenantHeaderAttribute
  /// Attaches this when AddSwaggerGen gets called
  /// </summary>
  /// <example>
  /// <code>
  /// services.AddSwaggerGen(c => {
  ///   c.OperationFilter&lt;SwaggerHeaderFilter&lt;();
  /// }
  /// </code>
  /// </example>
  public class SwaggerHeaderFilter : IOperationFilter {
    AppConfig _config;

    public SwaggerHeaderFilter(AppConfig config) {
      _config = config;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context) {
      operation.Parameters ??= new List<OpenApiParameter>();
      if (context.MethodInfo.GetCustomAttribute(typeof(TenantHeaderAttribute)) is TenantHeaderAttribute attribute) {
        var existingParam = operation.Parameters.FirstOrDefault(p => p.In == ParameterLocation.Header);
        // remove description from [FromHeader] argument attribute
        if (existingParam != null) {
          operation.Parameters.Remove(existingParam);
        }
        // NOTE: comment 'Required' out for now since when it's set to true, the Swagger UI wont work at all even when the field is filled already.
        // TODO: look into Swashbuckle on github...
        operation.Parameters.Add(new OpenApiParameter {
          Name = _config.TenantKey,
          In = ParameterLocation.Header,
          /*Required = true, */
          Description = attribute.Description,
          Schema = new OpenApiSchema {
            Type = "String"
          }
        });
      }
    }
  }

  public class OptionalPathParameterFilter : IOperationFilter {
    private readonly string _captureName = "routeParameter";

    public void Apply(OpenApiOperation operation, OperationFilterContext context) {
      var httpMethodAttributes = context.MethodInfo
        .GetCustomAttributes(true)
        .OfType<HttpMethodAttribute>();

      var httpMethodWithOptional = httpMethodAttributes?.FirstOrDefault(m => m.Template.Contains("?"));
      if (httpMethodWithOptional == null)
        return;

      string regex = $"{{(?<{_captureName}>\\w+)\\?}}";

      var matches = System.Text.RegularExpressions.Regex.Matches(httpMethodWithOptional.Template, regex);

      foreach (Match match in matches) {
        var name = match.Groups[_captureName].Value;

        var parameter = operation.Parameters.FirstOrDefault(p => p.In == ParameterLocation.Path && p.Name == name);
        if (parameter != null) {
          parameter.AllowEmptyValue = true;
          parameter.Description = "Must check \"Send empty value\" or Swagger passes a comma for empty values otherwise";
          parameter.Required = false;
          parameter.Schema.Nullable = true;
        }
      }
    }
  }
}