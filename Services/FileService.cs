using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;

namespace MultiTenancy {
  public class FileEntity {
    public string Key { get; init; }
    public string Url { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
  }

  public interface IEntityHasFiles : IEntity {
    List<FileEntity> Documents { get; set; }
  }

  public class FileService {
    private readonly AmazonS3Client _s3;
    private readonly AppConfig _appConfig;
    private readonly ApplicationContext _ctx;

    public FileService(AppConfig config, ApplicationContext ctx) {
      _appConfig = config;
      _ctx = ctx;

      var accessKey = _appConfig.Config["AWS:AccessKey"];
      var secretKey = _appConfig.Config["AWS:SecretKey"];
      _s3 = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.GetBySystemName(Region));
    }

    public string Region {
      get => _appConfig.Config["AWS:Region"];
    }

    public string S3Bucket {
      get => _appConfig.Config["AWS:S3Bucket"];
    }

    public string GetUrl(string key) => $"https://{S3Bucket}.s3-{Region}.amazonaws.com/{key}";

    public async Task<bool> DeleteFile(string key) {
      try {
        var deleteRequest = new DeleteObjectRequest {
          BucketName = S3Bucket,
          Key = key
        };
        await _s3.DeleteObjectAsync(deleteRequest);
        return true;
      } catch (AmazonS3Exception e) {
        throw HttpException.BadRequest(e.Message);
      }
    }

    public async Task<FileEntity> UploadFile(IFormFile file, string prefix, string name, params string[] acceptedExtensions) {
      try {
        string extension = file.FileName.FileExtension();
        if (!acceptedExtensions.IsEmpty()) {
          bool accepted = false;

          acceptedExtensions.Loop((ae, _) => {
            accepted = extension.CompareStandard(ae);
            return !accepted;
          });

          if (!accepted) throw HttpException.BadRequest($"only accept file with extensions {acceptedExtensions.Flatten(',')}");
        }

        string key = Path.Combine(
          _ctx.TryGetTenantHostname(),
          prefix,
          Guid.NewGuid().ToString() + Path.GetExtension(file.FileName)
        );

        using (var ms = new MemoryStream()) {
          await file.CopyToAsync(ms);

          var uploadRequest = new TransferUtilityUploadRequest {
            InputStream = ms,
            Key = key,
            BucketName = S3Bucket,
            CannedACL = S3CannedACL.PublicRead,
            ContentType = file.ContentType
          };

          await new TransferUtility(_s3).UploadAsync(uploadRequest);

          return new FileEntity {
            Key = key,
            Url = GetUrl(key),
            Name = name.ToUnderscoreLower(),
            Type = file.ContentType
          };
        }
      } catch (AmazonS3Exception e) {
        throw HttpException.BadRequest(e.Message);
      }
    }

    public async Task<FileEntity> UploadAndSaveToDb<TEntity, TDbContext>(TenantRepository<TEntity, TDbContext> repo, TEntity entity, IFormFile file, string prefix, string name, params string[] acceptedExtensions)
    where TEntity : class, IEntityHasFiles
    where TDbContext : TenantDbContext {
      // check if exists in the list , delete by its key
      var exists = entity.Documents?.RemoveWhere(x => x.Name.CompareStandard(name));
      if (!exists.IsEmpty()) {
        foreach (var exist in exists) {
          await DeleteFile(exist.Key);
        }
      }

      var fileEntity = await UploadFile(file, prefix, name, acceptedExtensions);
      Utils.If(
        entity.Documents == null,
        () => entity.Documents = new List<FileEntity>() { fileEntity },
        () => entity.Documents.Add(fileEntity)
      );
      // need to hack this by re-newing the list, since it's a jsonb type. 
      // otherwise EF wont save it since it somehow thinks that nothing gets changed, 
      // prolly bcause it thinks the reference of the list is still the same, hence ignored.
      entity.Documents = new List<FileEntity>(entity.Documents);

      await repo.UpdateAndSave(entity);

      return fileEntity;
    }
  }
}