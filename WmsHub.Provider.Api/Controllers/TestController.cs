using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Models;
using WmsHub.Provider.Api.Filters;

namespace WmsHub.Provider.Api.Controllers;

[ApiController]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/[controller]")]
[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(AuthenticationSchemes = "ApiKey")]
[FeatureGate(Feature.TestOnlyApi)]
public class TestController : BaseController
{
  private readonly IMapper _mapper;
  public TestController(IProviderService providerService, IMapper mapper)
    : base(providerService)
  {
    _mapper = mapper;
  }

  /// <summary>
  /// Creates between 1 and 200 test referrals (default 20) with an initial 
  /// provider selected status if they are not already present
  /// </summary>
  /// <response code="200">Test referrals created</response>
  /// <response code="400">Invalid request</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="403">Forbidden</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [HttpPost]
  [DevelopmentEnvironmentOnly]
  public async Task<IActionResult> CreateTestReferrals(
    [FromQuery]int num = 20, bool withHistory = false, 
    bool setBadContactNumbers = false,
    bool setAsRmcCall = false,
    bool skipExistingCheck = false)
  {
    try
    {
      if (num < 1 || num > 200)
      {
        return BadRequest(
          $"Can only created between 1 and 200 test referrals not {num}.");
      }

      Referral[] createdReferrals =
        await Service.CreateTestReferralsAsync(
          num, withHistory, setBadContactNumbers,
          setAsRmcCall, skipExistingCheck);

      List<dynamic> resultSet = new();
      foreach (Referral referral in createdReferrals)
      {
        dynamic item = new ExpandoObject();
        item.Ubrn = referral.Ubrn;
        item.Id = referral.Id;
        item.Telephone = referral.Telephone;
        item.Mobile = referral.Mobile;
        resultSet.Add(item);
      }

      if (createdReferrals.Any())
      {
        return Ok(resultSet);
      }
      else
      {
        return BadRequest("Test referrals already exist.");
      }
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  /// <summary>
  /// Deletes all test referrals if they are present
  /// </summary>
  /// <response code="200">Test referrals deleted</response>
  /// <response code="400">Invalid request</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="403">Forbidden</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response> 
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [HttpDelete]
  [DevelopmentEnvironmentOnly]
  public async Task<IActionResult> DeleteTestReferrals()
  {
    try
    {
      return await Service.DeleteTestReferralsAsync() 
        ? Ok() 
        : BadRequest("No test referrals available to delete.");
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  /// <summary>
  /// Creates over 10,000 User Action logs in the databse after removing any
  /// previous fake logs
  /// </summary>
  /// <response code="200">Test logs created</response>
  /// <response code="400">Invalid request</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="403">Forbidden</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [HttpPut]
  [DevelopmentEnvironmentOnly]
  public async Task<IActionResult> PutUserActionLogs(
    [FromQuery]bool justDelete = false)
  {
    try
    {
      dynamic response = new ExpandoObject();
      int deletedRows = await Service.DeleteTestUserActionAsync();
      
      if (!justDelete)
      {
        int createdRows = await Service.CreateTestUserActionAsync();
        response.CreatedRows = createdRows;
      }

      response.DeletedRows = deletedRows;
      return Ok(response);
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  /// <summary>
  /// Creates between 1 and 200 test referrals (default 20) with an initial 
  /// status CancelledByEreferrals.
  /// DateProvider Selected = DateTime.UtcNow.AddDays(-50).
  /// Not Started Programme = true.
  /// DateStartedProgramme = DateTime.UtcNow.AddDays(-50).
  /// Source = ReferralSource.SelfReferral as default.
  /// 
  /// If you set the "allRandom = true" then all inputs are set to
  /// status CancelledByEreferrals or Complete.
  /// DateProvider Selected = DateTime.UtcNow.AddDays(-50 to -300).
  /// Not Started Programme = true or false.
  /// DateStartedProgramme = DateTime.UtcNow.AddDays(-50 to -300).
  /// Source = Any source.
  /// </summary>
  /// <response code="200">Test referrals created</response>
  /// <response code="400">Invalid request</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="403">Forbidden</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response>
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [HttpPost]
  [Route("CreateTestReEntryReferrals")]
  [DevelopmentEnvironmentOnly]
  public async Task<IActionResult> CreateTestReEntryReferrals(
    [FromQuery] int num = 200,
    int providerSelectedAddDays = -50,
    bool notStarted = true,
    int startedProgrammeAddDays = -50,
    string referralStatus = "CancelledByEreferrals",
    bool allRandom = true)
  {
    try
    {
      if (num < 1 || num > 200)
      {
        return BadRequest(
          $"Can only created between 1 and 200 test referrals not {num}.");
      }

      Referral[] createdReferrals =
        await Service.CreateTestCompleteReferralsAsync(
          num,
          providerSelectedAddDays,
          notStarted,
          startedProgrammeAddDays,
          referralStatus,
          allRandom);

      List<dynamic> resultSet = new();
      foreach (Referral referral in createdReferrals)
      {
        dynamic item = new ExpandoObject();
        item.StatusReason = referral.StatusReason;
        item.Ubrn = referral.Ubrn;
        item.NhsNumber = referral.NhsNumber;
        item.Id = referral.Id;
        item.Status = referral.Status;
        item.Source = referral.ReferralSource;
        item.ProviderId = referral.ProviderId;
        item.DateOfProviderSelection = referral.DateOfProviderSelection;
        item.DateStartedProgramme = referral.DateStartedProgramme;
        resultSet.Add(item);
      }

      if (createdReferrals.Any())
      {
        return Ok(resultSet);
      }
      else
      {
        return BadRequest("Test referrals already exist.");
      }
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  protected internal virtual IProviderService Service
  {
    get
    {
      IProviderService service = _service as IProviderService;
      service.User = User;
      return service;
    }
  }
}
