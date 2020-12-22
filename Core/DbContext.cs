using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;

namespace MultiTenancy {
  /// <summary>
  /// The Tenant DB Context.  
  /// </summary>
  public class TenantDbContext : DbContext {
    protected readonly int _tenantId = -1;

    private readonly HttpContext _httpContext;
    private readonly IConfiguration _config;

    protected TenantDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor, IConfiguration config) : base(options) {
      _config = config;
      _httpContext = httpContextAccessor.HttpContext;
      if (_httpContext != null && _httpContext.Request.Headers.ContainsKey("TenantId")) {
        _tenantId = int.Parse(_httpContext.Request.Headers["TenantId"]);
      }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      // Configure default schema
      modelBuilder.HasDefaultSchema(_config["Multitenancy:Schema"]);
      // set default value for created_at as well as updated_at
      modelBuilder.EntitiesOfType<IEntity>(b => {
        b.Property<DateTimeOffset>(nameof(IEntity.CreatedAt)).HasDefaultValueSql("now()");
        b.Property<DateTimeOffset>(nameof(IEntity.UpdatedAt)).HasDefaultValueSql("now()");
      });
      // filters out the entity that is scoped to a tenant automagically
      // so that when it gets queried, it only returns the entity for the particular tenant
      // TODO: create a generic stuff of this for better reusability.
      var baseFilter = (Expression<Func<IEntity, bool>>)(_ => false);
      var tenantFilter = (Expression<Func<ITenantScopedEntity, bool>>)(e => e.CompanyId == _tenantId);
      var clrTypes = modelBuilder.Model.GetEntityTypes().Select(et => et.ClrType).ToList();
      foreach (var type in clrTypes) {
        var filters = new List<LambdaExpression>();

        if (typeof(ITenantScopedEntity).IsAssignableFrom(type)) filters.Add(tenantFilter);

        var queryFilter = CombineQueryFilters(type, baseFilter, filters);
        modelBuilder.Entity(type).HasQueryFilter(queryFilter);
      }
      // call the base
      base.OnModelCreating(modelBuilder);
    }

    private LambdaExpression CombineQueryFilters(Type entityType, LambdaExpression baseFilter, IEnumerable<LambdaExpression> andAlsoExpressions) {
      var newParam = Expression.Parameter(entityType);

      var andAlsoExprBase = (Expression<Func<IEntity, bool>>)(_ => true);
      var andAlsoExpr = ReplacingExpressionVisitor.Replace(andAlsoExprBase.Parameters.Single(), newParam, andAlsoExprBase.Body);
      foreach (var expressionBase in andAlsoExpressions) {
        var expression = ReplacingExpressionVisitor.Replace(expressionBase.Parameters.Single(), newParam, expressionBase.Body);
        andAlsoExpr = Expression.AndAlso(andAlsoExpr, expression);
      }

      var baseExp = ReplacingExpressionVisitor.Replace(baseFilter.Parameters.Single(), newParam, baseFilter.Body);
      var exp = Expression.OrElse(baseExp, andAlsoExpr);

      return Expression.Lambda(exp, newParam);
    }
  }
}