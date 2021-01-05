using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MultiTenancy {
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
        var existingParam = operation.Parameters.FirstOrDefault(p =>
                        p.In == ParameterLocation.Header);
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
}