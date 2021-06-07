using System;
using System.Collections.Generic;

namespace Uploader.Models
{
  public record MpuResult(string UploadId);
  public record SignedPartUrls(IEnumerable<string> Urls, DateTime expires);

  public record CompletedUpload(string Bucket, string Key);
}