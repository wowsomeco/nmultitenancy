using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MultiTenancy {
  [ApiController]
  [Route("enums/[controller]")]
  [TypeFilter(typeof(TenantDomainFilterAttribute))]
  [TypeFilter(typeof(AuthFilterAttribute))]
  public class EnumsController<T, TDbContext> : ControllerBase
  where T : class, IEnumEntity, new()
  where TDbContext : TenantDbContext {
    private readonly TenantRepository<T, TDbContext> _repo;
    private readonly LocalCacheService _cache;

    public EnumsController(TenantRepository<T, TDbContext> repo, LocalCacheService cache) {
      _repo = repo;
      _cache = cache;
    }

    public string CacheKey {
      get => typeof(T).Name;
    }

    /// <summary>
    /// Get all enums.
    /// </summary>            
    [HttpGet]
    [TenantHeader]
    [ProducesJson(typeof(ICollection<EnumGetDto>))]
    public async Task<IActionResult> GetAll() {
      return Ok(await _GetAll());
    }

    /// <summary>
    /// Get enum by id.
    /// </summary>            
    [HttpGet("{id}")]
    [TenantHeader]
    [ProducesJson(typeof(ICollection<EnumGetDto>))]
    public async Task<IActionResult> GetById(string id) {
      return Ok(await _GetById(id));
    }

    /// <summary>
    /// Creates a new enum
    /// </summary>                
    [HttpPost]
    [TenantHeader]
    [ProducesJson(typeof(EnumGetDto))]
    public async Task<IActionResult> Insert([FromBody] EnumActionDto body) {
      return Ok(await _Insert(body));
    }

    /// <summary>
    /// Updates an enum by id.    
    /// </summary>                
    [HttpPut("{id}")]
    [TenantHeader]
    [ProducesJson(typeof(EnumGetDto))]
    [ProducesValidationError]
    public async Task<IActionResult> Update(string id, [FromBody] EnumActionDto body) {
      return Ok(await _Update(id, body));
    }

    /// <summary>
    /// Deletes an enum by id.
    /// </summary>            
    [HttpDelete("{id}")]
    [TenantHeader]
    [ProducesJson(typeof(DeleteResponse))]
    public async Task<IActionResult> Delete(string id) {
      return Ok(await _Delete(id));
    }

    protected List<EnumGetDto> GetFromCache() {
      List<EnumGetDto> items = null;
      if (_cache.TryGetValue(CacheKey, out items)) {
        return items;
      }

      return null;
    }

    [HttpGet("count")]
    [TenantHeader]
    [ProducesJson(typeof(CountResponse))]
    public async Task<IActionResult> Count() {
      return Ok(new CountResponse { Count = await _repo.Table.CountAsync() });
    }

    protected void StoreToCache(List<EnumGetDto> items) {
      _cache.SetValue(CacheKey, items);
    }

    protected async Task UpdateCache(EnumGetDto item) {
      List<EnumGetDto> items = await _GetAll();

      var it = items.Find(x => x.Id == item.Id);

      if (it == null) {
        items.Add(item);
      } else {
        it.Name = item.Name;
      }

      StoreToCache(items);
    }

    protected async Task DeleteCache(string id) {
      List<EnumGetDto> items = await _GetAll();
      items.RemoveWhere(x => x.Id == id);

      StoreToCache(items);
    }

    protected async Task<List<EnumGetDto>> _GetAll() {
      List<EnumGetDto> cache = GetFromCache();
      if (cache != null) {
        return cache;
      }

      var result = await _repo.GetAll(
        e => EnumGetDto.FromEntity(e)
      );

      StoreToCache(result);

      return result;
    }

    protected async Task<EnumGetDto> _GetById(string id) {
      List<EnumGetDto> items = await _GetAll();
      EnumGetDto item = items.Find(x => x.Id == id) ?? throw HttpException.NotExists(id);

      return item;
    }

    protected async Task<EnumGetDto> _Insert(EnumActionDto body) {
      string id = body.Name.ToUnderscoreLower();

      var result = await _repo.Insert(
        body.ToEntity<T>(),
        e =>
          !Utils.If(
            _repo.Table.AlreadyExists(x => x.Id == id),
            () =>
              throw HttpException.AlreadyExists($"{id}", "sudah ada di database")
          )
      );

      var dto = EnumGetDto.FromEntity(result);
      await UpdateCache(dto);

      return dto;
    }

    protected async Task<EnumGetDto> _Update(string id, [FromBody] EnumActionDto body) {
      var result = await _repo.Update(
        t => {
          t.Name = body.Name;
        },
        () => $"{id}",
        x => x.Id == id
      );

      var dto = EnumGetDto.FromEntity(result);
      await UpdateCache(dto);

      return dto;
    }

    protected async Task<DeleteResponse> _Delete(string id) {
      var deleted = await _repo.Delete(id);
      if (deleted) {
        await DeleteCache(id);
      }

      return new DeleteResponse { Deleted = deleted };
    }
  }
}