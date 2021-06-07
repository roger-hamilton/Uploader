using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Web6.Models;

namespace Web6.Interfaces
{
  public interface IMultiPartUploader
  {
    const int MinimumMpuPartSize = 5_242_880; // 5 Mb

    TimeSpan DefaultExpiration { get; }
    DateTime CurrentExpiration { get; }
    Task<MpuResult> CreateMultiPartUpload(string name, CancellationToken cancellationToken = default);

    Task AbortMultiPartUpload(string name, string uploadId, CancellationToken cancellationToken = default);

    SignedPartUrls CreateSignedPartUrls(string name, string uploadId, int fileSize, int partSize = MinimumMpuPartSize);

    Task<CompletedUpload> CompleteMultiPartUpload(string name, string uploadId, IEnumerable<string> etags, CancellationToken cancellationToken = default);

    Task<int> AbortExpired(bool force, CancellationToken cancellationToken = default);
  }
}