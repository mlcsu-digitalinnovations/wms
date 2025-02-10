using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.BusinessIntelligence;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Controllers;

namespace WmsHub.BusinessIntelligence.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[Route("[Controller]")]
public class ReferralController : BaseController
{
  private readonly IMapper _mapper;
  public ReferralController(IBusinessIntelligenceService businessIntelligenceService,
    IMapper mapper)
    : base(businessIntelligenceService)
  {
    _mapper = mapper;
  }

  /// <summary>
  /// Get a list of the count of referrals by each month.
  /// </summary>
  /// <returns>An enumerable of ReferralCountByMonth.</returns>
  [Authorize(Policy = AuthPolicies.ReferralCountsAuthPolicy.POLICYNAME)]
  [HttpGet("counts")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [Produces("application/json")]
  public async Task<ActionResult<IEnumerable<ReferralCountByMonth>>>
    GetReferralCountsByMonth()
  {
    try
    {
      IEnumerable<ReferralCountByMonth> counts = await Service
        .GetReferralCountsByMonthAsync();

      return Ok(counts);
    }
    catch(Exception ex)
    {
      LogException(ex);
      return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
    }
  }

  /// <summary>
  /// Lists all referrals with optional filtering by referral date.
  /// </summary>
  /// <remarks>All referrals returned are anonymised. If the optional filter 
  /// parameters are not provided then data is returned for the previous 
  /// 31 days only. If only one filter parameter is used then the other 
  /// parameter will default to 31 days before or after the provided 
  /// parameter.</remarks>
  /// <response code="200">Request successful.</response>
  /// <response code="204">No data returned.</response>
  /// <response code="400">Bad request.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="500">Internal server error.</response>
  /// <response code="503">Service unavailable, please try again.</response> 
  /// <param name="fromDate">(Optional) filter list to include only referrals
  /// with a referral date equal to and after this date and time.</param>
  /// <param name="toDate">(Optional) filter list to include only referrals
  /// with a referral date equal to and before this date and time.</param>
  [Authorize(Policy = AuthPolicies.DefaultAuthPolicy.POLICYNAME)]
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [Produces("application/json")]
  public async Task<ActionResult<IEnumerable<Models.AnonymisedReferral>>>
    Get(
    [FromQuery] DateTimeOffset? fromDate,
    [FromQuery] DateTimeOffset? toDate)
  {
    GetAzureSocketIp();

    //filter date check
  if (fromDate.HasValue && toDate.HasValue)
    {
      if (fromDate > toDate)
      {
        return Problem(
          detail: $"'from' date {fromDate} cannot be "
                  + $"later than 'to' date {toDate}.",
          statusCode: StatusCodes.Status400BadRequest);
      }
    }
    else if (fromDate.HasValue && !toDate.HasValue)
    {
      toDate = fromDate.Value.AddDays(31);
    }
    else if (!fromDate.HasValue && toDate.HasValue)
    {
      fromDate = toDate.Value.AddDays(-31);
    }
    else
    {
      toDate = DateTimeOffset.Now;
      fromDate = toDate.Value.AddDays(-31);
    }

    IBusinessIntelligenceService busIntService = Service;
    List<Models.AnonymisedReferral> AnonymisedReferralviewModel;

    try
    {
      IEnumerable<Business.Models.AnonymisedReferral> anonReferralDtos =
        await busIntService.GetAnonymisedReferrals(fromDate, toDate);

      if (anonReferralDtos.Count() == 0)
        return NoContent();

      AnonymisedReferralviewModel = (List<Models.AnonymisedReferral>)
        _mapper.Map(
          anonReferralDtos,
          typeof(IEnumerable<Business.Models.AnonymisedReferral>),
          typeof(IEnumerable<Models.AnonymisedReferral>)
      );
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
    }

    return Ok(AnonymisedReferralviewModel);
  }

  /// <summary>
  /// Lists all referrals with filtering by ModifiedAt date and ProviderSubmission date.
  /// </summary>
  /// <remarks>
  /// All referrals returned are anonymised.
  /// </remarks>
  /// <response code="200">Request successful.</response>
  /// <response code="204">No data returned.</response>
  /// <response code="400">Bad request.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="500">Internal server error.</response>
  /// <response code="503">Service unavailable, please try again.</response> 
  /// <param name="lastDownloadDate">
  /// Filter list to include only referrals
  /// with a ModifiedAt date or ProviderSubmission Date equal to and after this date.
  /// </param>
  [Authorize(Policy = AuthPolicies.DefaultAuthPolicy.POLICYNAME)]
  [HttpGet("changes")]
  [ProducesResponseType(StatusCodes.Status200OK, 
    Type = typeof(IEnumerable<Models.AnonymisedReferral>))]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [Produces("application/json")]
  public async Task<IActionResult> GetChanges(
    [FromQuery] DateTimeOffset? lastDownloadDate)
  {
    GetAzureSocketIp();

    //filter date check
    if (!lastDownloadDate.HasValue)
    {
        return Problem(
          detail: $"{nameof(lastDownloadDate)} is required.",
          statusCode: StatusCodes.Status400BadRequest);
    }
    else if (lastDownloadDate.HasValue
      && lastDownloadDate > DateTimeOffset.Now)
    {
      return Problem(
        detail: $"{nameof(lastDownloadDate)} cannot be in future.",
        statusCode: StatusCodes.Status400BadRequest);
    }

    List<Models.AnonymisedReferral> anonymisedReferralViewModels;

    try
    {
      IEnumerable<Business.Models.AnonymisedReferral> anonymisedReferrals =
        await Service.GetAnonymisedReferralsChangedFromDate(lastDownloadDate.Value);

      if (!anonymisedReferrals.Any())
      {
        return NoContent();
      }

      anonymisedReferralViewModels = (List<Models.AnonymisedReferral>)
        _mapper.Map(
          anonymisedReferrals,
          typeof(IEnumerable<Business.Models.AnonymisedReferral>),
          typeof(IEnumerable<Models.AnonymisedReferral>)
      );
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(statusCode: StatusCodes.Status500InternalServerError);
    }

    return Ok(anonymisedReferralViewModels);
  }

  /// <summary>
  /// When an elective care file is processed and if it has any errors, these
  /// are recorded in the ElectiveCarePostErrors table.  This will return the
  /// raw saved data based on the filters.
  /// <br />
  /// All the validations errors only recorded which property failed and does
  /// not record any data from errored cell.
  /// </summary>
  /// <param name="fromDate">From date will default to 31 days in the past
  /// </param>
  /// <param name="toDate">To date will default to today.</param>
  /// <param name="trustOdsCode">If the ODS code is not supplied,
  /// then the result will not be filtered by ODS code</param>
  /// <param name="trustUserId">If the Trust User Id is not supplied, or is of
  /// Guid.Empty, then results are not filtered by the user.</param>
  /// <returns>The raw table data is return in descending order with the
  /// latest at the top of the file.  The columns return are:
  /// <list type="bullet">
  /// <item><description>Id - Integer as a generic counter</description></item>
  /// <item><description>PostError - is the raw error as raised through
  /// validation.</description></item>
  /// <item><description>RowNumber - is the row number of the uploaded
  /// spreadsheet.</description></item>
  /// <item><description>TrustOdsCode - ODS code as supplied.
  /// </description></item>
  /// <item><description>TrustUserId is the GUID User ID as supplied.
  /// </description></item>
  /// </list>
  /// </returns>
  [Authorize(Policy = AuthPolicies.DefaultAuthPolicy.POLICYNAME)]
  [HttpGet("ElectiveCarePostErrors")]
  [ProducesResponseType(
    StatusCodes.Status200OK,
    Type = typeof(
    IEnumerable<Business.Models.ElectiveCareReferral.ElectiveCarePostError>))]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  public async Task<IActionResult> GetElectiveCarePostErrors(
    [FromQuery] DateTimeOffset? fromDate,
    [FromQuery] DateTimeOffset? toDate,
    [FromQuery] string trustOdsCode,
    [FromQuery] Guid? trustUserId)
  {
    GetAzureSocketIp();
    if (fromDate.HasValue && toDate.HasValue)
    {
      if (fromDate > toDate)
      {
        return Problem(
          detail: $"'from' date {fromDate} cannot be "
                  + $"later than 'to' date {toDate}.",
          statusCode: StatusCodes.Status400BadRequest);
      }
    }
    else if (fromDate.HasValue && !toDate.HasValue)
    {
      toDate = fromDate.Value.AddDays(31);
    }
    else if (!fromDate.HasValue && toDate.HasValue)
    {
      fromDate = toDate.Value.AddDays(-31);
    }
    else
    {
      toDate = DateTimeOffset.Now;
      fromDate = toDate.Value.AddDays(-31);
    }

    try
    {
      IEnumerable<Business.Models.ElectiveCareReferral.ElectiveCarePostError>
        electiveCarePostErrors = await Service.GetElectiveCarePostErrorsAsync(
          fromDate,
          toDate,
          trustOdsCode,
          trustUserId);

      return Ok(electiveCarePostErrors);
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
    }
  }

  [Authorize(Policy = AuthPolicies.DefaultAuthPolicy.POLICYNAME)]
  [HttpGet("providerrejected")]
  [ProducesResponseType(StatusCodes.Status200OK,
    Type = typeof(IEnumerable<Models.AnonymisedReferral>))]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [Produces("application/json")]
  public async Task<IActionResult> ProviderRejectedReferrals(
    [FromQuery] DateTimeOffset? fromDate,
    [FromQuery] DateTimeOffset? toDate)
  {
    GetAzureSocketIp();

    try
    {
      ValidateDate(ref fromDate, ref toDate);
      IEnumerable<Business.Models.AnonymisedReferral> anonReferralDtos =
        await Service.GetAnonymisedReferralsByProviderReason(
          fromDate, 
          toDate, 
          ReferralStatus.ProviderRejected | 
          ReferralStatus.ProviderRejectedTextMessage);

      if (anonReferralDtos == null || !anonReferralDtos.Any())
      {  
        return NoContent();
      }

      IEnumerable<Models.AnonymisedReferral> anonymisedReferralViewModel =
        _mapper.Map<
          IEnumerable<Business.Models.AnonymisedReferral>, 
          IEnumerable<Models.AnonymisedReferral>>(
            anonReferralDtos);

      return Ok(anonymisedReferralViewModel);
    }
    catch (Exception ex)
    {
      if (ex is DateRangeNotValidException)
      {
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status400BadRequest);
      }

      return Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  [Authorize(Policy = AuthPolicies.DefaultAuthPolicy.POLICYNAME)]
  [HttpGet("providerdeclined")]
  [ProducesResponseType(StatusCodes.Status200OK,
  Type = typeof(IEnumerable<Models.AnonymisedReferral>))]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [Produces("application/json")]
  public async Task<IActionResult> ProviderDeclinedReferrals(
  [FromQuery] DateTimeOffset? fromDate,
  [FromQuery] DateTimeOffset? toDate)
  {
    GetAzureSocketIp();

    try
    {
      ValidateDate(ref fromDate, ref toDate);
      IEnumerable<Business.Models.AnonymisedReferral> anonReferralDtos =
       await Service.GetAnonymisedReferralsByProviderReason(
         fromDate,
         toDate,
         ReferralStatus.ProviderDeclinedByServiceUser |
         ReferralStatus.ProviderDeclinedTextMessage);
      if (anonReferralDtos == null || !anonReferralDtos.Any())
      {
        return NoContent();
      }

      IEnumerable<Models.AnonymisedReferral> anonymisedReferralViewModel =
        _mapper.Map<
          IEnumerable<Business.Models.AnonymisedReferral>,
          IEnumerable<Models.AnonymisedReferral>>(
            anonReferralDtos);

      return Ok(anonymisedReferralViewModel);
    }
    catch (Exception ex)
    {
      if (ex is DateRangeNotValidException)
      {
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status400BadRequest);
      }

      return Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  [Authorize(Policy = AuthPolicies.DefaultAuthPolicy.POLICYNAME)]
  [HttpGet("providerterminated")]
  [ProducesResponseType(StatusCodes.Status200OK,
  Type = typeof(IEnumerable<Models.AnonymisedReferral>))]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  [Produces("application/json")]
  public async Task<IActionResult> ProviderTerminatedReferrals(
  [FromQuery] DateTimeOffset? fromDate,
  [FromQuery] DateTimeOffset? toDate)
  {
    GetAzureSocketIp();

    try
    {
      ValidateDate(ref fromDate, ref toDate);

      IEnumerable<Business.Models.AnonymisedReferral> anonReferralDtos =
      await Service.GetAnonymisedReferralsByProviderReason(
        fromDate,
        toDate,
        ReferralStatus.ProviderTerminated |
        ReferralStatus.ProviderTerminatedTextMessage);

      if (anonReferralDtos == null || !anonReferralDtos.Any())
      {
        return NoContent();
      }

      IEnumerable<Models.AnonymisedReferral> anonymisedReferralViewModel =
        _mapper.Map<
          IEnumerable<Business.Models.AnonymisedReferral>,
          IEnumerable<Models.AnonymisedReferral>>(
            anonReferralDtos);

      return Ok(anonymisedReferralViewModel);
    }
    catch (Exception ex)
    {
      if (ex is DateRangeNotValidException)
      {
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status400BadRequest);
      }
      else if ( ex is ReferralInvalidStatusException)
      {
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }

      return Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  ///// <summary>
  ///// Lists all providers, their submission errors and referral awaiting 
  ///// acceptance stats.
  ///// </summary>
  ///// <remarks>If the optional filter parameters are not provided then data 
  ///// is returned for the previous 31 days only. If only one filter parameter
  ///// is used then the other parameter will default to 31 days before or 
  ///// after the provided parameter.</remarks>
  ///// <response code="200">Request successful.</response>
  ///// <response code="204">No data returned.</response>
  ///// <response code="400">Bad request.</response>
  ///// <response code="401">Invalid authentication.</response>
  ///// <response code="500">Internal server error.</response>
  ///// <response code="503">Service unavailable, please try again.</response> 
  ///// <param name="ubrn">this date and time.</param>
  // ND - THIS IS BEING REMOVED UNTIL FURTHER NOTICE
  //[HttpGet("History")]
  //[ProducesResponseType(StatusCodes.Status200OK)]
  //[ProducesResponseType(StatusCodes.Status204NoContent)]
  //[ProducesResponseType(StatusCodes.Status400BadRequest)]
  //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
  //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
  //[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  //[Produces("application/json")]
  //public async 
  //  Task<ActionResult<IEnumerable<Models.AnonymisedReferralHistory>>>
  //  Get(string ubrn)
  //{
  //  GetAzureSocketIp();

  //  //filter date check
  //  if (string.IsNullOrWhiteSpace(ubrn))
  //  {
  //      return Problem(
  //        detail: $"'UBRN must be provided",
  //        statusCode: StatusCodes.Status400BadRequest); 
  //  }

  //  BusinessIntelligenceService busIntService = Service;
  //  List<Models.AnonymisedReferralHistory> models;

  //  try
  //  {
  //    IEnumerable<Business.Models.AnonymisedReferral> dtos =
  //      await busIntService.GetAnonymisedReferralsForUbrn(ubrn);

  //    if (dtos.Count() == 0)
  //      return NoContent();

  //    models = (List<Models.AnonymisedReferralHistory>)
  //      _mapper.Map(
  //        dtos,
  //        typeof(IEnumerable<Business.Models.AnonymisedReferralHistory>),
  //        typeof(IEnumerable<Models.AnonymisedReferralHistory>)
  //      );
  //  }
  //  catch (Exception ex)
  //  {
  //    LogException(ex);
  //    return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
  //  }

  //  return Ok(models);
  //}

  private IBusinessIntelligenceService Service
  {
    get
    {
      var service = _service as IBusinessIntelligenceService;
      service.User = User;
      return service;
    }
  }

 
}

