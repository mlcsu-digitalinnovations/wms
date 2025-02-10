using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Models.ElectiveCareReferral;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Models.ElectiveCareReferral;

namespace WmsHub.Referral.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Authorize(Policy = AuthPolicies.ElectiveCare.POLICY_NAME)]
[Route("v{version:apiVersion}/[controller]")]
[Route("[Controller]")]
public class ElectiveCareReferralController : BaseController
{

  private readonly ILogger _log;
  private readonly IMapper _mapper;

  public ElectiveCareReferralController(
    IElectiveCareReferralService electiveCareReferralService,
    ILogger logger,
    IMapper mapper)
    : base(electiveCareReferralService)
  {
    _log = logger.ForContext<ElectiveCareReferralController>();
    _mapper = mapper;
  }

  /// <summary>
  /// Returns the quota details for the provided Ods code.
  /// </summary>
  /// <response code="200">The quota details were successfully returned.
  /// </response>
  /// <response code="400">The ODS code is invalid.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="403">Invalid authorization.</response>
  /// <response code="500">Internal server error.</response> 
  [HttpGet("{odsCode:length(3)}/quota")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetQuota(string odsCode)
  {
    try
    {
      GetQuotaDetailsResult result = await Service
        .GetQuotaDetailsAsync(odsCode.ToUpper());

      if (result.IsValid)
      {
        return Ok(
          new { result.OdsCode, result.QuotaTotal, result.QuotaRemaining });
      }
      else
      {
        _log.Debug("GetQuota failed: {error}", result.Error);
        return BadRequest(new {Detail = result.Error });
      }
      
    }
    catch (Exception ex)
    {
      _log.Error(ex, "GetQuota Failed.");

      return Problem(ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  /// <summary>
  /// Creates elective care referrals from an xls, xlsx or csv file.
  /// </summary>
  /// <param name="request">A PostRequest object containing details of the user
  /// uploading the file, their organisation and the file containing the 
  /// referral data.</param>
  /// <response code="200">The referrals were successfully created.</response>
  /// <response code="400">The PostRequest object failed validation.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="403">Invalid authorization.</response>
  /// <response code="422">Referral quota exceeded.</response>
  /// <response code="500">Internal server error.</response> 
  [HttpPost]
  [DisableRequestSizeLimit]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Post([FromForm] PostRequest request)
  {
    try
    {
      if (!await Service.UserHasAccessToOdsCodeAsync(
        request.TrustUserId,
        request.TrustOdsCode))
      {
        string message = $" User Id '{request.TrustUserId}' does not have " +
          $"access to ODS code '{request.TrustOdsCode}'.";

        _log.Debug(message);

        return Problem(
          detail: message,
          statusCode: StatusCodes.Status403Forbidden);
      }

      request.GetRowsFromFile();

      string detail = BuildDetailMessage(request);

      if (!request.HasHeaderRow)
      {
        detail += " Did not find a valid header row. " +
          "Please review the file and correct the column names and their " +
          "order so that they exactly match those that are in the example " +
          "blank templates. Then re-upload the file.";

        _log.Debug(detail);

        return Problem(
          detail: detail,
          statusCode: StatusCodes.Status400BadRequest);
      }

      if (!request.AllRows.Any()
        || !request.AllRows.Any(x => x.RowType == PostRequest.RowType.Data))
      {
        detail += " There are no rows that contain referrals. Please correct " +
          "the errors and re-upload the file.";

        _log.Debug(detail);

        return Problem(
          detail: detail,
          statusCode: StatusCodes.Status400BadRequest);
      }

      GetQuotaDetailsResult quotaDetails = await Service
        .GetQuotaDetailsAsync(request.TrustOdsCode);

      if (quotaDetails.IsValid)
      {
        if (quotaDetails.QuotaRemaining < request.DataRows.Count)
        {
          int reduceRowsBy =
             Math.Abs(quotaDetails.QuotaRemaining - request.DataRows.Count);

          detail += $" The remaining quota for {request.TrustOdsCode} is " +
            $"only {quotaDetails.QuotaRemaining}, but " +
            $"{request.DataRows.Count} have been uploaded. Please reduce " +
            $"the number of rows in the file by {reduceRowsBy} and re-upload " +
            $"the file.";

          _log.Debug(detail);

          return Problem(
            detail: detail,
            statusCode: StatusCodes.Status422UnprocessableEntity);
        }
      }
      else
      {
        detail += " " + quotaDetails.Error;

        _log.Debug(detail);

        return Problem(
          detail: detail,
          statusCode: StatusCodes.Status400BadRequest);
      }

      IEnumerable<ElectiveCareReferralTrustData> trustData = _mapper
        .Map<IEnumerable<ElectiveCareReferralTrustData>>(request.DataRows);

      ProcessTrustDataResult result = await Service
        .ProcessTrustDataAsync(trustData, request.TrustOdsCode, request.TrustUserId);

      if (result.IsValid)
      {

        detail += $" Created {result.NoOfReferralsCreated} referrals.";

        _log.Debug(detail);

        return Ok(new 
        { 
          Detail = detail,
          result.QuotaRemaining,
          result.QuotaTotal,
        });
      }
      else
      {
        detail += $" Found errors in {result.Errors.Count} out of " +
          $"{request.DataRows.Count} row(s), so no " +
          "referrals were created. Please remove rows that are " +
          "ineligible, correct any errors and re-upload the file.";

        _log.Debug(detail);

        return BadRequest(new { Detail = detail, RowErrors = result.Errors });
      }
    }
    catch (Exception ex)
    {
      _log.Error(ex, "Post Failed.");
      return Problem(ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  private static string BuildDetailMessage(PostRequest request)
  {
    string detail = $"Found {request.AllRows.Count} row(s).";

    if (request.HasEmptyRows)
    {
      detail += $" Ignored {request.EmptyRowsCount} empty row(s).";
    }
    if (request.HasHeaderRow)
    {
      detail += $" Found a valid header in the first row.";
    }

    return detail;
  }

  private IElectiveCareReferralService Service
  {
    get
    {
      IElectiveCareReferralService service =
        _service as IElectiveCareReferralService;
      service.User = User;
      return service;
    }
  }
}
