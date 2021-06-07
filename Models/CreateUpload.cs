using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Uploader.Models
{
  public record CreateUpload
  {
    [Required]
    public string Name { get; init; }
    [Required]
    public int FileSize { get; init; }
  }

  public record CreateUploadResponse
  {
    [Required]
    public string UploadId { get; init; }
    [Required]
    public IEnumerable<string> PartUrls { get; init; }
    [Required]
    public string CompleteUrl { get; init; }
    [Required]
    public string AbortUrl { get; init; }
    [Required]
    public int MinimumChunkSize { get; init; }
  }
}