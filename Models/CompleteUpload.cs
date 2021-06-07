using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Uploader.Models
{
  public record CompleteUpload
  {
    [Required]
    public string Name { get; set; }
    [Required]
    public IEnumerable<string> ETags { get; set; }
  }
}