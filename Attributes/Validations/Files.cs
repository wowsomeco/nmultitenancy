using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MultiTenancy {
  public class AllowedContentTypeAttribute : ValidationAttribute {
    private readonly string[] _contentTypes;

    public AllowedContentTypeAttribute(params string[] contentTypes) {
      _contentTypes = contentTypes;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext) {
      if (value is IFormFile file) {
        if (!_contentTypes.IsEmpty()) {
          bool accepted = false;

          _contentTypes.Loop((ae, _) => {
            accepted = file.ContentType.CompareStandard(ae);
            return !accepted;
          });

          if (!accepted) return new ValidationResult($"only accept file with content-type {_contentTypes.Flatten(',')}");
        }
      }

      return ValidationResult.Success;
    }
  }

  public class AllowContentImgAttribute : AllowedContentTypeAttribute {
    public AllowContentImgAttribute() : base(MimeTypes.Jpeg, MimeTypes.Png) { }
  }
}