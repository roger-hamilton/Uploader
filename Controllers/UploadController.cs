using System;
using System.Threading.Tasks;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Web6.Interfaces;
using Web6.Models;

namespace Web6.Controllers
{
  [Route("uploads")]
  public class UploadController : ControllerBase
  {
    private readonly IMultiPartUploader uploader;

    public UploadController(IMultiPartUploader uploader)
    {
      this.uploader = uploader;
    }

    [HttpPost]
    public async Task<ActionResult<CreateUploadResponse>> CreateUpload([FromBody] CreateUpload model)
    {
      if (!ModelState.IsValid) return BadRequest(ModelState);

      var mpu = await uploader.CreateMultiPartUpload(model.Name, HttpContext.RequestAborted);

      try
      {
        var urlResult = uploader.CreateSignedPartUrls(model.Name, mpu.UploadId, model.FileSize);

        return new CreateUploadResponse
        {
          UploadId = mpu.UploadId,
          PartUrls = urlResult.Urls,
          CompleteUrl = Url.Action(nameof(CompleteUpload), new { uploadId = mpu.UploadId }),
          AbortUrl = Url.Action(nameof(AbortUpload), new { uploadId = mpu.UploadId, name = model.Name }),
          MinimumChunkSize = IMultiPartUploader.MinimumMpuPartSize,
        };
      }
      catch (Exception)
      {
        await uploader.AbortMultiPartUpload(model.Name, mpu.UploadId);
        throw;
      }
    }

    [HttpDelete("{uploadId}/{name}")]
    public async Task<ActionResult> AbortUpload(string uploadId, string name)
    {
      await uploader.AbortMultiPartUpload(name, uploadId, HttpContext.RequestAborted);
      return Ok();
    }

    [HttpDelete("expired")]
    public async Task<ActionResult<int>> AbortExpired(bool force = false)
    {
      var count = await uploader.AbortExpired(force: force, HttpContext.RequestAborted);
      return count;
    }

    [HttpPost("{uploadId}")]
    public async Task<ActionResult<CompletedUpload>> CompleteUpload(string uploadId, [FromBody] CompleteUpload model)
    {
      if (!ModelState.IsValid) return BadRequest(model);
      var res = await uploader.CompleteMultiPartUpload(model.Name, uploadId, model.ETags, HttpContext.RequestAborted);

      return res;
    }
  }
}