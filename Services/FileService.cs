using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace MultiTenancy {
  public class CompressedFile {
    public MemoryStream Stream { get; set; }
    public int Width { get; set; }
  }

  public class FileEntity {
    public string Key { get; init; }
    public string Url { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
  }

  public class ImageEntity {
    public class Info {
      public string Url { get; set; }
      public int W { get; set; }
    }

    public string Key { get; set; }
    public List<Info> Contents { get; set; } = new List<Info>();

    public List<string> Keys => Contents.Map(c => $"{Key}_{c.W}.jpg");
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

      var accessKey = _appConfig["AWS:AccessKey"];
      var secretKey = _appConfig["AWS:SecretKey"];
      _s3 = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.GetBySystemName(Region));
    }

    public string Region => _appConfig["AWS:Region"];

    public string S3Bucket => _appConfig["AWS:S3Bucket"];

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

    public async Task<bool> DeleteFile(ImageEntity img) {
      foreach (var k in img?.Keys) {
        await DeleteFile(k);
      }

      return true;
    }

    public async Task<FileEntity> UploadFile(MemoryStream ms, string prefix, string name, string contentType) {
      try {
        string key = Path.Combine(
          _ctx.TryGetTenantHostname(),
          prefix,
          name
        );

        var uploadRequest = new TransferUtilityUploadRequest {
          InputStream = ms,
          Key = key,
          BucketName = S3Bucket,
          CannedACL = S3CannedACL.PublicRead,
          ContentType = contentType
        };

        await new TransferUtility(_s3).UploadAsync(uploadRequest);

        return new FileEntity {
          Key = key,
          Url = GetUrl(key),
          Name = name,
          Type = contentType
        };
      } catch (AmazonS3Exception e) {
        throw HttpException.BadRequest(e.Message);
      }
    }

    public async Task<FileEntity> UploadFile(IFormFile file, string prefix, string name, params string[] acceptedContentTypes) {
      // TODO: combine this with UploadFile method above
      try {
        string contentType = file.ContentType;
        if (!acceptedContentTypes.IsEmpty()) {
          bool accepted = false;

          acceptedContentTypes.Loop((ae, _) => {
            accepted = contentType.CompareStandard(ae);
            return !accepted;
          });

          if (!accepted) throw HttpException.BadRequest($"only accept file with content-type {acceptedContentTypes.Flatten(",")}");
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

          ms.Close();

          return new FileEntity {
            Key = key,
            Url = GetUrl(key),
            Name = name.ToDashLower(),
            Type = file.ContentType
          };
        }
      } catch (AmazonS3Exception e) {
        throw HttpException.BadRequest(e.Message);
      }
    }

    public async Task<FileEntity> UploadAndSaveToDb<TEntity, TDbContext>(TenantRepository<TEntity, TDbContext> repo, TEntity entity, IFormFile file, string prefix, string name, params string[] acceptedContentTypes)
    where TEntity : class, IEntityHasFiles
    where TDbContext : TenantDbContext {
      string standardizedName = name.ToDashLower();
      // check if exists in the list , delete by its key
      var exists = entity.Documents?.RemoveWhere(x => x.Name == standardizedName);
      if (!exists.IsEmpty()) {
        foreach (var exist in exists) {
          await DeleteFile(exist.Key);
        }
      }
      // upload to s3
      var fileEntity = await UploadFile(file, prefix, name, acceptedContentTypes);
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

    /// <summary>
    /// Uploads multiple size images from a file.
    /// Useful when you want to 
    /// </summary>
    /// <param name="file">The uploaded file</param>
    /// <param name="prefix">The prefix of the final filename</param>
    /// <param name="name">The filename</param>
    /// <param name="options">The array of tuple width and quality, the file will get compressed according to the length of this arr</param>
    /// <returns>The list of different file sizes after saved to s3</returns>
    public async Task<ImageEntity> UploadMultiSizeImages(IFormFile file, string prefix, string name, params (int width, int quality)[] options) {
      string key = Path.Combine(
        _ctx.TryGetTenantHostname(),
        prefix,
        name
      );

      ImageEntity imageEntity = new ImageEntity { Key = key };

      var resized = await ResizeMulti(file, options);
      foreach (var r in resized) {
        var f = await UploadFile(r.Stream, prefix, $"{name}_{r.Width}.jpg", "image/jpeg");
        imageEntity.Contents.Add(new ImageEntity.Info { Url = f.Url, W = r.Width });
      }

      return imageEntity;
    }

    public async Task<MemoryStream> Resize(IFormFile file, (int width, int quality) wq) {
      var memoryStream = new MemoryStream();
      using (var image = Image.Load(file.OpenReadStream())) {
        var beforeMutations = image.Size();
        // init resize object
        var resizeOptions = new ResizeOptions {
          Size = new Size(Math.Clamp(wq.width, 0, 1024), 0),
          Sampler = KnownResamplers.Lanczos3,
          Compand = true,
          Mode = ResizeMode.Max
        };
        // mutate image
        image.Mutate(x => {
          if (file.ContentType == MimeTypes.Png) {
            x.BackgroundColor(Color.White);
          }

          x.Resize(resizeOptions);
        });
        await image.SaveAsync(memoryStream, new JpegEncoder { Quality = wq.quality });

        return memoryStream;
      }
    }

    public async Task<List<CompressedFile>> ResizeMulti(IFormFile file, params (int width, int quality)[] options) {
      var streams = new List<CompressedFile>();

      foreach (var wq in options) {
        var ms = await Resize(file, wq);
        streams.Add(new CompressedFile { Stream = ms, Width = wq.width });
      }

      return streams;
    }
  }
}