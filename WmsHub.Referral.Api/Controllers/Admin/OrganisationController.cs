using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Referral.Api.Models;
using WmsHub.Referral.Api.Models.Admin;

namespace WmsHub.Referral.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/admin/[controller]")]
[Route("admin/[Controller]")]
[Authorize(Policy = AuthPolicies.Admin.POLICY_NAME)]
public class OrganisationController : ControllerBase
{
  private readonly IDateTimeProvider _dateTimeProvider;
  private readonly ILogger _log;
  /// <summary>
  /// Don't use this field in methods, instead use the Service property so 
  /// the User is added to the service.
  /// </summary>
  private readonly IOrganisationService _service;
  
  

  public OrganisationController(
    IDateTimeProvider dateTimeProvider,
    ILogger log,
    IOrganisationService service)
  {
    _dateTimeProvider = dateTimeProvider;
    _service = service;
    _log = log.ForContext<OrganisationController>();
  }

  [HttpDelete("{odsCode:length(3)}")]
  public async Task<IActionResult> Put(string odsCode)
  {
    try
    {
      await Service.DeleteAsync(odsCode);

      return Ok();
    }
    catch (InvalidOperationException ex)
    {
      _log.Debug(ex, "Failed to delete organisation.");
      return Problem(ex.Message,
        statusCode: StatusCodes.Status400BadRequest);
    }
    catch (Exception ex)
    {
      _log.Error(ex, "Error deleting organisations.");
      return Problem(ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  [HttpGet]
  public async Task<IActionResult> Get()
  {
    try
    {
      return Ok(await Service.GetAsync());
    }
    catch (InvalidOperationException ex)
    {
      _log.Debug(ex, "Failed to get organisation.");
      return Problem(ex.Message,
        statusCode: StatusCodes.Status400BadRequest);
    }
    catch (Exception ex)
    {
      _log.Error(ex, "Error getting organisations.");
      return Problem(ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  [HttpPost()]
  public async Task<IActionResult> Post(OrganisationPostRequest request)
  {
    try
    {
      Organisation organisation = new()
      {
        OdsCode = request.OdsCode,
        QuotaRemaining = request.QuotaRemaining,
        QuotaTotal = request.QuotaTotal,
      };

      organisation = await Service.AddAsync(organisation);

      return Ok(organisation);
    }
    catch (InvalidOperationException ex)
    {
      _log.Debug(ex, "Failed to create organisation.");
      return Problem(ex.Message,
        statusCode: StatusCodes.Status400BadRequest);
    }
    catch (Exception ex)
    {
      _log.Error(ex, "Error creating organisation.");
      return Problem(ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  [HttpPut("{odsCode:length(3)}")]
  public async Task<IActionResult> Put(
    [FromRoute] string odsCode, 
    [FromBody] OrganisationPutRequest request)
  {
    try
    {
      Organisation organisation = new()
      {
        OdsCode = odsCode,
        QuotaRemaining = request.QuotaRemaining,
        QuotaTotal = request.QuotaTotal,
      };

      organisation = await Service.UpdateAsync(organisation);

      return Ok(organisation);
    }
    catch (InvalidOperationException ex)
    {
      _log.Debug(ex, "Failed to update organisation.");
      return Problem(ex.Message,
        statusCode: StatusCodes.Status400BadRequest);
    }
    catch (Exception ex)
    {
      _log.Error(ex, "Error updating organisation.");
      return Problem(ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  [HttpPost("resetorganisationquotas")]
  public async Task<IActionResult> ResetOrganisationQuotas(
    [FromQuery] bool overrideDate = false)
  {
    {
      try
      {

        if (_dateTimeProvider.Now.Day == 1 || overrideDate)
        { 
          await Service.ResetOrganisationQuotas();
        }
        else 
        {
          return Problem(
            detail: "Error only able to reset organisation quotas on 1st of " +
            "month.",
            statusCode: StatusCodes.Status422UnprocessableEntity);
        }

        return Ok();
      }
      catch (Exception ex)
      {
        _log.Error(ex, "Error resetting organisation quotas.");
        return Problem(ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }
  }

  private IOrganisationService Service
  {
    get
    {
      _service.User = User;
      return _service;
    }
  }
}
