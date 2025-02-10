using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;

namespace WmsHub.Referral.Api.Controllers.Admin
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/admin/[controller]")]
  [Route("admin/[Controller]")]
  public class GeneratorController : BaseController
  {
    public GeneratorController(IReferralService service)
      : base(service)
    { }

    /// <summary>
    /// Returns a list of valid NHS Numbers starting with 999
    /// </summary>
    /// <param name="required">
    /// Integer: sets the number of NHS Numbers required. (max 1000)</param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Get(int? required = 1)
    {
      try
      {
        const int MAX_NUMBERS = 1000;
        if (required > MAX_NUMBERS)
        {
          return BadRequest($"Cannot request {required} NHS numbers, the " +
            $"maximum requestable is {MAX_NUMBERS}.");
        }

        string[] nhsNumbers = Service.GetNhsNumbers(required);

        return Ok(nhsNumbers);
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Failed to get {required} NHS numbers.", required);
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    private ReferralService Service
    {
      get
      {
        ReferralService service = _service as ReferralService;
        service.User = User;
        return service;
      }
    }
  }
}
