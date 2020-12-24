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
  /// </summary>
  /// <example>
  /// <code>
  /// </code>
  /// </example>
  public class TenantRepository<TEntity, TDbContext>
  where TEntity : class, IEntity where TDbContext : TenantDbContext {
    private TenantDbContext _context = null;
    private DbSet<TEntity> _table = null;

    public DbSet<TEntity> Table {
      get {
        return _table;
      }
    }

    public TenantRepository(TDbContext ctx) {
      _context = ctx;
      _table = _context.Set<TEntity>();
    }

    public async Task<List<T>> GetAll<T>(Func<TEntity, T> transformer, params Expression<Func<TEntity, bool>>[] filters) {
      if (null != filters) {
        foreach (var filter in filters) {
          _table.Where(filter);
        }
      }

      var entities = await _table.ToListAsync();
      return entities.Map(e => transformer(e));
    }

    public async Task<T> GetOne<T>(Expression<Func<TEntity, bool>> predicate, Func<TEntity, T> callback, Func<string> onNotFound) {
      var entity = await _table.FirstOrDefaultAsync(predicate) ?? throw HttpException.NotExists(onNotFound());
      return callback(entity);
    }

    public async Task<TEntity> Insert(TEntity model, InsertCondition<TEntity> condition = null) {
      condition?.Invoke(model);

      // check if the current entity is a scoped entity.
      // automagically assign the tenant id if so
      // TODO: find a way to handle hostname not found... 
      // right now the error is thrown directly from Postgres i.e. 23503 (foreign_key_violation)
      // which can be confusing...
      model.TryCastTo<TEntity, ITenantScopedEntity>(scoped => {
        scoped.TenantId = _context.AppContext.TenantHostname;
      });

      _table.Add(model);
      await Save();

      return model;
    }

    public async Task<TEntity> Update(Expression<Func<TEntity, bool>> predicate, UpdateCallback<TEntity> callback, Func<string> onErr) {
      var entity = await GetOne(predicate, e => e, onErr);

      callback(entity);
      _table.Attach(entity);

      await Save();
      return entity;
    }

    public async Task<bool> Delete(int id) {
      TEntity t = _table.Find(id);
      _table.Remove(t);
      return await Save() > 0;
    }

    public async Task<int> Save() {
      try {
        return await _context.SaveChangesAsync();
      } catch (Exception e) {
        throw new HttpException(HttpStatusCode.BadRequest, e.InnerException.Message);
      }
    }
  }
}