using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WmsHub.ErsMock.Api.Models;

namespace WmsHub.ErsMock.Api.Controllers;

[ApiController]
[Route("ers-api/STU3/v1/[controller]")]
public class AttachmentController(IOptions<ErsMockApiOptions> options) : ControllerBase
{
  private readonly ErsMockApiOptions _options = options.Value;

  [HttpGet("{url}")]
  public IActionResult A006RetrieveAttachment([FromRoute] string url)
  {
    string pdfFullPath = Path.Combine(_options.AttachmentReferralLetterPath, $"{url}.pdf");
    string rtfFullPath = Path.Combine(_options.AttachmentReferralLetterPath, $"{url}.rtf");

    string foundFullPath;
    string contentType;

    if (System.IO.File.Exists(pdfFullPath))
    {
      foundFullPath = pdfFullPath;
      contentType = "application/pdf";
    }
    else if (System.IO.File.Exists(rtfFullPath))
    {
      foundFullPath = rtfFullPath;
      contentType = "application/rtf";
    }
    else
    {
      return NotFound();
    }

    MemoryStream attachmentStream = new(System.IO.File.ReadAllBytes(foundFullPath));
    return new FileStreamResult(attachmentStream, contentType);
  }
}
