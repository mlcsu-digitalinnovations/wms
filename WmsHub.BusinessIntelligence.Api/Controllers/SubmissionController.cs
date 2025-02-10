using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Controllers;

namespace WmsHub.BusinessIntelligence.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Policy = AuthPolicies.DefaultAuthPolicy.POLICYNAME)]
[Route("v{version:apiVersion}/[controller]")]
[Route("[Controller]")]
[SwaggerTag("Provider submission related methods.")]
public class SubmissionController : BaseController
{
  private readonly IMapper _mapper;
  public SubmissionController(IBusinessIntelligenceService busIntelliService,
    IMapper mapper)
    : base(busIntelliService)
  {
    _mapper = mapper;
  }

  /// <summary>
  /// Lists all referrals with optional filtering by provider submission date.
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
  /// that have submissions whose submission date is equal to and after this 
  /// date and time.</param>
  /// <param name="toDate">(Optional) filter list to include only referrals
  /// that have submissions whose submission date is equal to and before this 
  /// date and time.</param>
  [HttpGet]
  [SwaggerResponse(
    200,
    "Lists all referrals with optional filtering by provider submission " +
      "date.",
    typeof(IEnumerable<Models.AnonymisedReferral>))]
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

    // Filter date check.
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
      List<Models.AnonymisedReferral> AnonymisedReferralviewModel;
      IEnumerable<Business.Models.AnonymisedReferral> anonReferralDtos =
        await Service.GetAnonymisedReferralsBySubmissionDate(fromDate,
          toDate);

      if (anonReferralDtos.Count() == 0)
      {
        return NoContent();
      }

      AnonymisedReferralviewModel = (List<Models.AnonymisedReferral>)
        _mapper.Map(
          anonReferralDtos,
          typeof(IEnumerable<Business.Models.AnonymisedReferral>),
          typeof(IEnumerable<Models.AnonymisedReferral>)
      );

      return Ok(AnonymisedReferralviewModel);
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(detail: ex.Message,
        statusCode: (int)HttpStatusCode.InternalServerError);
    }
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
  [HttpGet("changes")]
  [SwaggerResponse(200,
    "Lists all referrals that have been modified since the provided date.",
    typeof(IEnumerable<Models.AnonymisedReferral>))]
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

    // Filter date check.
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

    List<Models.AnonymisedReferral> AnonymisedReferralViewModels;

    try
    {
      IEnumerable<Business.Models.AnonymisedReferral> anonymisedReferrals =
        await Service.GetAnonymisedReferralsChangedFromDate(lastDownloadDate.Value);

      if (!anonymisedReferrals.Any())
      {
        return NoContent();
      }

      AnonymisedReferralViewModels = (List<Models.AnonymisedReferral>)
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

    return Ok(AnonymisedReferralViewModels);
  }

  [HttpGet("ended")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
  public IActionResult EndedSubmissions(
    [FromQuery] DateTimeOffset? fromDate,
    [FromQuery] DateTimeOffset? toDate)
  {
    GetAzureSocketIp();

    try
    {
      ValidateDate(ref fromDate, ref toDate);

      Business.Models.ProviderEndedData providerEnded =   
        Service.ProviderEndedReasonStats(fromDate, toDate);

      if (providerEnded == null)
      {
        throw new ArgumentNullException(nameof(providerEnded));
      }

      return Ok(providerEnded);
    }
    catch (Exception ex)
    {
      if (ex is DateRangeNotValidException)
      {
        return Problem(
          detail: ex.Message,
          statusCode: StatusCodes.Status400BadRequest);
      }
      else if (ex is ReferralInvalidStatusException)
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

