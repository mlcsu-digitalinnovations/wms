using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;
using WmsHub.Common.Apis.Ods.PostcodesIo;

namespace WmsHub.Referral.Api.Controllers.Admin
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/admin/[controller]")]
  [Route("admin/[Controller]")]
  public class PostcodeController : ControllerBase
  {
    private readonly IPostcodesIoService _service;

    public PostcodeController(IPostcodesIoService service)
    {
      _service = service;
    }

    [HttpGet]
    [Route("{postcode}")]
    public async Task<IActionResult> GetLsoa(string postcode)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(postcode))
        {
          return BadRequest("Postcode not provided");
        }

        string lsoa = await _service.GetLsoaAsync(postcode);

        if (lsoa == null)
        {
          return NotFound();
        }
        else
        {
          return Ok(lsoa);
        }        
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Failed to get lsoa for {postcode}.", postcode);

        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }
  }
}