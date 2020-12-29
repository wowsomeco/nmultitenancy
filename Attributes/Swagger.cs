using System;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace MultiTenancy {
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
  public class ProducesJsonAttribute : ProducesAttribute {
    public ProducesJsonAttribute(Type type) : base(MediaTypeNames.Application.Json) {
      Type = type;
    }
  }
}