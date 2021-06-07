using System;
using System.Collections.Generic;

namespace Web6.Models
{
  public record MpuResult(string UploadId);
  public record SignedPartUrls(IEnumerable<string> Urls, DateTime expires);

  public record CompletedUpload(string Bucket, string Key);
}