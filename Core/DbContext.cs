using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace MultiTenancy {
  /// <summary>
  /// The Tenant DB Context.
  /// This sets query filter for all the tables that inherit ITenantScopedEntity by AppContext.TenantHostname
  /// Subclass this and create your own DbContext accordingly.
  ///   
  /// </summary>
  public class TenantDbContext : DbContext {
    public ApplicationContext AppContext {
      get; private set;
    }

    protected TenantDbContext(DbContextOptions options, ApplicationContext appContext) : base(options) {
      AppContext = appContext;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      // Configure default schema
      modelBuilder.HasDefaultSchema(AppContext.Config.DbSchema);
      // set default value for created_at as well as updated_at
      modelBuilder.EntitiesOfType<IEntity>(b => {
        b.Property<DateTimeOffset>(nameof(IEntity.CreatedAt)).HasDefaultValueSql("now()");
        b.Property<DateTimeOffset>(nameof(IEntity.UpdatedAt)).HasDefaultValueSql("now()");
      });
      // filters out the entity that is scoped to a tenant automagically
      // so that when it gets queried, it only returns the entity for the particular tenant
      // TODO: create a generic stuff of this for better reusability.
      var baseFilter = (Expression<Func<IEntity, bool>>)(_ => false);
      var tenantFilter = (Expression<Func<ITenantScopedEntity, bool>>)(e => e.TenantId == AppContext.TenantHostname);
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