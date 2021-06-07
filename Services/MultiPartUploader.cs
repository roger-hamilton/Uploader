using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.Extensions.Options;
using Uploader.Models;
using Uploader.Extensions;
using System.Linq;
using Amazon.S3.Model;
using Uploader.Interfaces;
using Microsoft.Extensions.Logging;

namespace Uploader.Services
{
  public class MultiPartUploader : IMultiPartUploader
  {
    private readonly IAmazonS3 s3;
    private readonly IOptions<S3Config> s3Config;
    private readonly ILogger<MultiPartUploader> logger;

    public MultiPartUploader(
      IAmazonS3 s3,
      IOptions<S3Config> s3Config,
      ILogger<MultiPartUploader> logger
    )
    {
      this.s3 = s3;
      this.s3Config = s3Config;
      this.logger = logger;
    }

    public TimeSpan DefaultExpiration => TimeSpan.FromHours(2);
    public DateTime CurrentExpiration => DateTime.Now + DefaultExpiration;
    public string KeyFromName(string name) => $"{s3Config.Value.KeyPrefix}{name}";

    private string BucketName => s3Config.Value.BucketName;

    public async Task<MpuResult> CreateMultiPartUpload(string name, CancellationToken cancellationToken = default)
    {
      logger.LogInformation("Creating new MPU for {Name}: {@Config}", name, new { BucketName, Key = KeyFromName(name) });
      var res = await s3.InitiateMultipartUploadAsync(
        bucketName: BucketName,
        key: KeyFromName(name),
        cancellationToken
      );
      return new MpuResult(res.UploadId);
    }

    public async Task AbortMultiPartUpload(string name, string uploadId, CancellationToken cancellationToken = default)
    {
      await s3.AbortMultipartUploadAsync(
        bucketName: BucketName,
        key: KeyFromName(name),
        uploadId: uploadId,
        cancellationToken: cancellationToken
      );
    }

    public SignedPartUrls CreateSignedPartUrls(string name, string uploadId, int fileSize, int partSize = IMultiPartUploader.MinimumMpuPartSize)
    {
      if (fileSize <= 0) throw new ArgumentException("Must be greater than 0", nameof(fileSize));
      if (partSize < IMultiPartUploader.MinimumMpuPartSize) throw new ArgumentException($"Must be greater than {IMultiPartUploader.MinimumMpuPartSize}", nameof(partSize));

      var numParts = (int)Math.Ceiling(fileSize / (double)partSize);

      var expires = CurrentExpiration;

      var urls = Enumerable.Range(1, numParts)
        .Select(n => s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
          BucketName = BucketName,
          Key = KeyFromName(name),
          UploadId = uploadId,
          Expires = expires,
          Verb = HttpVerb.PUT,
          PartNumber = n
        }));

      return new SignedPartUrls(urls, expires);
    }

    public async Task<CompletedUpload> CompleteMultiPartUpload(string name, string uploadId, IEnumerable<string> etags, CancellationToken cancellationToken = default)
    {
      var parts = etags.Select((etag, idx) => new PartETag
      {
        ETag = etag,
        PartNumber = idx + 1,
      }).ToList();

      var res = await s3.CompleteMultipartUploadAsync(
        new CompleteMultipartUploadRequest
        {
          BucketName = BucketName,
          Key = KeyFromName(name),
          UploadId = uploadId,
          PartETags = parts,
        },
        cancellationToken
      );

      return new CompletedUpload(res.BucketName, res.Key);
    }

    public async Task<int> AbortExpired(bool force, CancellationToken cancellationToken = default)
    {
      var mpuParger = s3.Paginators.ListMultipartUploads(new ListMultipartUploadsRequest
      {
        BucketName = BucketName,
      });

      var count = 0;

      await foreach (var mpu in mpuParger.Uploads.WithCancellation(cancellationToken))
      {
        if (force || mpu.Initiated < DateTime.Now - DefaultExpiration)
        {
          await s3.AbortMultipartUploadAsync(BucketName, mpu.Key, mpu.UploadId, cancellationToken);
          count++;
        }
      }
      return count;
    }
  }
}