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

  public interface ITenantRepository<TEntity, TDbContext>
  where TEntity : class where TDbContext : TenantDbContext {
    DbSet<TEntity> Table { get; }
    Task<List<TEntity>> GetAll();
    Task<TEntity> GetOne(Expression<Func<TEntity, bool>> predicate, Func<string> onNotFound);
    Task<TEntity> Insert(TEntity model, InsertCondition<TEntity> condition = null);
    Task<TEntity> Update(Func<TEntity, bool> predicate, UpdateCallback<TEntity> callback, Func<string> notFound);
    Task<bool> Delete(int id);
  }

  public class TenantRepository<TEntity, TDbContext>
  : ITenantRepository<TEntity, TDbContext> where TEntity : class, IEntity where TDbContext : TenantDbContext {
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

    public Task<List<TEntity>> GetAll() {
      return _table.ToListAsync();
    }

    public Task<TEntity> GetOne(Expression<Func<TEntity, bool>> predicate, Func<string> onNotFound) {
      var entity = _table.FirstOrDefaultAsync(predicate);

      if (null == entity) {
        throw HttpException.NotExists(onNotFound());
      }

      return entity;
    }

    public async Task<TEntity> Insert(TEntity model, InsertCondition<TEntity> condition = null) {
      condition?.Invoke(model);

      _table.Add(model);
      await Save();

      return model;
    }

    public async Task<TEntity> Update(Func<TEntity, bool> predicate, UpdateCallback<TEntity> callback, Func<string> onErr) {
      var entity = _table.Where(predicate).FirstOrDefault();

      if (null == entity) {
        throw HttpException.NotExists(onErr());
      }

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