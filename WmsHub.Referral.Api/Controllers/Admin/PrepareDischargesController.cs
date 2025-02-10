using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Referral.Api.Models;

namespace WmsHub.Referral.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/admin/[controller]")]
[Route("admin/[Controller]")]
public class PrepareDischargesController(
  IReferralDischargeService service,
  IProcessStatusService processStatusService,
  IOptions<ProcessStatusOptions> processStatusOptions) : BaseController(service)
{
  private readonly IProcessStatusService _processStatusService = processStatusService;
  private readonly ProcessStatusOptions _processStatusOptions = processStatusOptions.Value;

  /// <summary>
  /// Prepares GP referrals ready for discharge
  /// </summary>
  /// <returns>The number of referrals prepared for discharge</returns>
  /// <response code="200">Referrals prepared</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [HttpGet]
  public async Task<IActionResult> Get()
  {
    try
    {
      _processStatusService.AppName = _processStatusOptions.PrepareDischargesAppName;
      await _processStatusService.StartedAsync();
    }
    catch (Exception ex)
    {
      LogException(ex);
    }

    try
    {
      string result = await Service.PrepareDischarges();
      await _processStatusService.SuccessAsync();
      return Ok(result);
    }
    catch (Exception ex)
    {
      LogException(ex);
      await _processStatusService.FailureAsync(ex.Message);
      return Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  private ReferralDischargeService Service
  {
    get
    {
      ReferralDischargeService service = _service as ReferralDischargeService;
      service.User = User;
      return service;
    }
  }
}
