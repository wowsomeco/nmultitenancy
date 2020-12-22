using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MultiTenancy {
  public interface ITenantRepository<T, U> {
    Task<List<T>> GetAll();
    Task<T> GetById(int id);
    Task<int> Insert(T model);
    Task<int> Update(T model);
    Task<int> Delete(int id);
  }

  public class TenantRepository<T, U>
  : ITenantRepository<T, U> where T : class where U : TenantDbContext {
    private TenantDbContext _context = null;
    private DbSet<T> _table = null;

    public TenantRepository(U ctx) {
      _context = ctx;
      _table = _context.Set<T>();
    }

    public Task<List<T>> GetAll() {
      return _table.ToListAsync();
    }

    public async Task<T> GetById(int id) {
      var item = await _table.FindAsync(id);
      if (item == null) {
        throw new HttpException(HttpStatusCode.NotFound, $"id = {id} does not exist in {typeof(T).Name}");
      }

      return item;
    }

    public Task<int> Insert(T model) {
      _table.Add(model);
      return Save();
    }

    public Task<int> Update(T model) {
      _table.Attach(model);
      return Save();
    }

    public Task<int> Delete(int id) {
      T t = _table.Find(id);
      _table.Remove(t);
      return Save();
    }

    public Task<int> Save() {
      return _context.SaveChangesAsync();
    }
  }
}