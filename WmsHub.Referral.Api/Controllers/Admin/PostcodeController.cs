using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Services;

namespace WmsHub.Referral.Api.Controllers.Admin
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/admin/[controller]")]
  [Route("admin/[Controller]")]
  public class PostcodeController : ControllerBase
  {
    private readonly IPostcodeService _service;

    public PostcodeController(IPostcodeService service)
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
          return BadRequest("Postcode not provided");

        return Ok(await _service.GetLsoa(postcode));
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Failed to GetLsoa for {postcode}.", postcode);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }
  }
}