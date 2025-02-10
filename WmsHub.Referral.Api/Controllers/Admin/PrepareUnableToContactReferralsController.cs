using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;

namespace WmsHub.Referral.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/admin/[controller]")]
[Route("admin/[Controller]")]
public class PrepareUnableToContactReferralsController : BaseController
{
  public PrepareUnableToContactReferralsController(IReferralService service)
  : base(service)
  { }

  /// <summary>
  /// Update any referrals with a Status of TextMessage3 and a DateOfReferral at least 42 days
  /// ago to have an intermediate Status of FailedToContact before updating to AwaitingDischarge
  /// (for Msk or GP referrals) or Complete (for other referrals).
  /// </summary>
  /// <returns>The number of referrals prepared</returns>
  /// <response code="200">Referrals prepared</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpGet]
  public async Task<IActionResult> Get()
  {
    try
    {
      string refsUpdated = await Service.PrepareFailedToContactAsync();

      return Ok(refsUpdated);
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  private IReferralService Service
  {
    get
    {
      IReferralService service = _service as IReferralService;
      service.User = User;
      return service;
    }
  }
}
