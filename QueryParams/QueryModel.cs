using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace MultiTenancy {
  public class QueryModel {
    [FromQuery(Name = "limit")]
    public int? Limit { get; set; }
    [FromQuery(Name = "offset")]
    public int? Offset { get; set; }

    public IQueryable<T> ToQueryable<T>(IQueryable<T> q) {
      if (Offset is int o) q = q.Skip(o);
      if (Limit is int l) q = q.Take(l);

      return q;
    }
  }
}