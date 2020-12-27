using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MultiTenancy {
  public delegate bool InsertCondition<TEntity>(TEntity entity);
  public delegate void UpdateCallback<TEntity>(TEntity entity);

  /// <summary>
  /// The Tenant Repository.
  /// TODO: more docs
  /// </summary>
  /// <example>
  /// <code>
  /// public void ConfigureServices(IServiceCollection services) {
  ///   services.AddTransient(typeof(TenantRepository));  
  /// }  
  /// </code>
  /// </example>
  public class TenantRepository<TEntity, TDbContext>
  where TEntity : class, IEntity where TDbContext : TenantDbContext {
    private DbSet<TEntity> _table = null;

    public DbSet<TEntity> Table {
      get {
        return _table;
      }
    }

    public TenantDbContext Context { get; init; }

    public ApplicationContext AppContext {
      get {
        return Context.AppContext;
      }
    }

    /// <summary>
    /// Callback that gets executed before making a get query.
    /// e.g. you can define the relationship e.g. Include(), etc. here
    /// </summary>    
    public Func<IQueryable<TEntity>, IQueryable<TEntity>> BeforeQuery { get; set; } = null;

    public TenantRepository(TDbContext ctx) {
      Context = ctx;
      _table = Context.Set<TEntity>();
    }

    /// <summary>
    /// Gets all the entities that matches the query filters.
    /// If no filters are defined, it will get all the entities from the database 
    /// </summary>
    /// <param name="transformer">The callback that transforms the original entity into TResult</param>
    /// <param name="filters">The query filter(s)</param>
    /// <typeparam name="TResult">The class that you want to transform entity into</typeparam>
    /// <returns>The list of TResult</returns>
    public async Task<List<TResult>> GetAll<TResult>(Func<TEntity, TResult> transformer, params Expression<Func<TEntity, bool>>[] filters) {
      var q = GenerateQuery(filters);
      var entities = await q.ToListAsync();
      return entities.Map(e => transformer(e));
    }

    /// <summary>
    /// Gets one row from the database and transforms it into TResult
    /// </summary>
    /// <param name="transformer">The callback that transforms the original entity into T</param>
    /// <param name="onNotFound">The message that gets thrown when the entity does not exist in the db</param>
    /// <param name="filters">The query filter(s)</param>
    /// <typeparam name="TResult">The class that you want to transform entity into</typeparam>
    /// <returns>TResult</returns>
    public async Task<TResult> GetOne<TResult>(Func<TEntity, TResult> transformer, Func<string> onNotFound, params Expression<Func<TEntity, bool>>[] filters) {
      var q = GenerateQuery(filters);

      var entity = await q.FirstOrDefaultAsync() ?? throw HttpException.NotExists(onNotFound());
      return transformer(entity);
    }

    /// <summary>
    /// Inserts a new entity to the database
    /// </summary>
    /// <param name="model">The entity to insert</param>
    /// <param name="condition">The condition that gets called prior to inserting, if returns false, it wont proceed and returns null immediately</param>
    /// <returns>The newly inserted entity</returns>
    public async Task<TEntity> Insert(TEntity model, InsertCondition<TEntity> condition = null) {
      bool shouldInsert = null != condition ? condition.Invoke(model) : true;
      if (!shouldInsert) return null;
      // check if the current entity is a scoped entity.
      // automagically assign the tenant id if so
      // TODO: find a way to handle hostname not found... 
      // right now the error is thrown directly from Postgres i.e. 23503 (foreign_key_violation)
      // which can be confusing...
      model.TryCastTo<TEntity, ITenantScopedEntity>(scoped => scoped.TenantId = Context.AppContext.TenantHostname);

      _table.Add(model);
      await Save();

      return model;
    }

    public async Task<TEntity> Update(UpdateCallback<TEntity> callback, Func<string> onNotFound, params Expression<Func<TEntity, bool>>[] filters) {
      var entity = await GetOne(e => e, onNotFound, filters);
      // gets called so the caller can update this cur entity accordingly
      callback(entity);
      // auto update the updated at column
      entity.TryCastTo<TEntity, IEntity>(ent => ent.UpdatedAt = DateTime.Now);
      // save to the database and return the entity
      _table.Attach(entity);
      await Save();
      return entity;
    }

    public async Task<bool> Delete(int id) {
      TEntity t = await _table.FindAsync(id);
      _table.Remove(t);
      return await Save() > 0;
    }

    public async Task<int> Count(params Expression<Func<TEntity, bool>>[] filters) {
      IQueryable<TEntity> q = GenerateQuery(filters);
      return await q.CountAsync();
    }

    public async Task<int> Save() {
      try {
        return await Context.SaveChangesAsync();
      } catch (Exception e) {
        throw new HttpException(HttpStatusCode.BadRequest, e.InnerException.Message);
      }
    }

    public IQueryable<TEntity> GenerateQuery(params Expression<Func<TEntity, bool>>[] filters) {
      IQueryable<TEntity> q = BeforeQuery == null ? _table : BeforeQuery(_table);

      if (null != filters) {
        foreach (var filter in filters) {
          q = q.Where(filter);
        }
      }

      return q;
    }
  }
}