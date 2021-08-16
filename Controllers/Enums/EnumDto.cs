using FluentValidation;

namespace MultiTenancy {
  public interface IEnumEntity : ITenantScopedEntity {
    string Id { get; set; }
    string Name { get; set; }
  }

  public class EnumGetDto {
    public string Id { get; set; }
    public string Name { get; set; }

    public static EnumGetDto FromEntity<T>(T e) where T : IEnumEntity {
      return new EnumGetDto {
        Id = e.Id,
        Name = e.Name
      };
    }
  }

  public class EnumActionDto {
    public string Name { get; set; }

    public T ToEntity<T>(string tenantId) where T : IEnumEntity, new() {
      var e = new T();
      e.TenantId = tenantId;
      e.Id = Name.ToUnderscoreLower();
      e.Name = Name;

      return e;
    }

    public static T ToEntity<T>(string tenantId, string name) where T : IEnumEntity, new() {
      return new EnumActionDto { Name = name }.ToEntity<T>(tenantId);
    }
  }

  public class EnumValidator : AbstractValidator<EnumActionDto> {
    public EnumValidator() {
      RuleFor(x => x.Name).NotEmpty().NoSpecialCharacters();
    }
  }
}