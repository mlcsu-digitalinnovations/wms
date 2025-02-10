using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Controllers;
using WmsHub.Referral.Api.Models.Admin;

namespace WmsHub.Referral.Api.Controllers.Admin;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/admin/[controller]")]
[Route("admin/[Controller]")]
[Authorize(Policy = AuthPolicies.Admin.POLICY_NAME)]
public class MskOrganisationController : BaseController
{
  private readonly ILogger _logger;

  public MskOrganisationController(
    IMskOrganisationService service,
    ILogger logger)
    : base(service)
  {
    _logger = logger.ForContext<MskOrganisationController>();
  }

  [HttpDelete("{odsCode}")]
  public async Task<IActionResult> Delete(string odsCode)
  {
    try
    {
      string result = await Service.DeleteAsync(odsCode);

      return Ok(result);
    }
    catch (Exception ex)
    {
      if (ex is MskOrganisationNotFoundException)
      {
        _logger.Debug(ex, "Failed to delete organisation.");
        return Problem(ex.Message,
          statusCode: StatusCodes.Status404NotFound);
      }

      _logger.Error(ex, "Error deleting organisations.");
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
    catch (Exception ex)
    {
      _logger.Error(ex, "Error getting organisations.");
      return Problem(ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  [HttpPost()]
  public async Task<IActionResult> Post(MskOrganisationPostRequest request)
  {
    try
    {
      MskOrganisation organisation = new()
      {
        OdsCode = request.OdsCode,
        SendDischargeLetters = request.SendDischargeLetters,
        SiteName = request.SiteName
      };

      organisation = await Service.AddAsync(organisation);

      return Ok(organisation);
    }
    catch (Exception ex)
    {
      if (ex is MskOrganisationValidationException)
      {
        _logger.Debug(ex, "Validation error.");
        return Problem(ex.Message,
          statusCode: StatusCodes.Status400BadRequest);
      }

      if (ex is InvalidOperationException)
      {
        _logger.Debug(ex, "Failed to create organisation.");
        return Problem(ex.Message,
          statusCode: StatusCodes.Status400BadRequest);
      }

      _logger.Error(ex, "Error creating organisation.");
      return Problem(ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  [HttpPut("{odsCode}")]
  public async Task<IActionResult> Put(
    [FromRoute] string odsCode,
    [FromBody] MskOrganisationPutRequest request)
  {
    if (string.IsNullOrWhiteSpace(odsCode))
    {
      return BadRequest(new ArgumentNullException(nameof(odsCode)));
    }

    try
    {
      MskOrganisation organisation = new()
      {
        OdsCode = odsCode,
        SendDischargeLetters = request.SendDischargeLetters,
        SiteName = request.SiteName
      };

      organisation = await Service.UpdateAsync(organisation);

      return Ok(organisation);
    }
    catch (Exception ex)
    {
      if (ex is MskOrganisationValidationException)
      {
        _logger.Debug(ex, "Validation error.");
        return Problem(ex.Message,
          statusCode: StatusCodes.Status400BadRequest);
      }

      if (ex is MskOrganisationNotFoundException)
      {
        _logger.Debug(ex, "Failed to delete organisation.");
        return Problem(ex.Message,
          statusCode: StatusCodes.Status404NotFound);
      }

      _logger.Error(ex, "Error updating organisation.");
      return Problem(ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  private IMskOrganisationService Service
  {
    get
    {
      _service.User = User;
      return (IMskOrganisationService)_service;
    }
  }
}
