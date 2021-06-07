namespace Uploader.Models
{
  public class S3Config
  {
    public string BucketName { get; set; }
    public string KeyPrefix { get; set; } = "uploads/";
  }
}