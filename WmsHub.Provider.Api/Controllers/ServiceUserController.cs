using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Models.ReferralStatusReason;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Extensions;
using WmsHub.Provider.Api.Filters;
using WmsHub.Provider.Api.Models;

namespace WmsHub.Provider.Api.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[HideDeprecated]
[Route("v{version:apiVersion}/[controller]")]
[Route("[controller]")]
public class ServiceUserController : BaseController
{
  private readonly IMapper _mapper;

  public ServiceUserController(IProviderService providerService, IMapper mapper)
    : base(providerService)
  {
    _mapper = mapper;
  }

  /// <summary>
  /// Returns a list of service users that have chosen the current provider.
  /// As soon as the service user makes their choice they will appear in 
  /// this list until the provider sends an Update Service Users request 
  /// that contains the service user’s id with a started status.
  /// </summary>
  /// <response code="200">Service user list returned</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="403">Forbidden</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response> 
  /// <param name="from">Optional date parameter. This NO longer filters
  /// the service user list, and though it can still be added as a 
  /// parameter its value is ignored.</param> 
  /// <returns>An array of service users awaiting programme start</returns>
  [HttpGet]  
  [MapToApiVersion("1.0")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<IEnumerable<ServiceUserResponse>>> Get(
    [FromQuery] DateTimeOffset? from)
  {
    // ND 17/01/2022 from date is no longer used because it stops providers
    // seeing referrals that have been stuck in the system for a while.
    if (from.HasValue && from > DateTimeOffset.Now.AddSeconds(5))
    {
      return Problem(
        detail: $"from date cannot be in the future {from}",
        statusCode: StatusCodes.Status400BadRequest);
    }

    return await Get();
  }

  /// <summary>
  /// Returns a list of service users that have chosen the current provider.
  /// As soon as the service user makes their choice they will appear in 
  /// this list until the provider sends an Update Service Users request 
  /// that contains the service user’s id with a started status.
  /// </summary>
  /// <response code="200">Service user list returned</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="403">Forbidden</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response> 
  /// <returns>An array of service users awaiting programme start</returns>
  [HttpGet]
  [MapToApiVersion("2.0")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<IEnumerable<ServiceUserResponse>>> Get()
  {
    IEnumerable<ServiceUser> serviceUsers = await Service.GetServiceUsers();

    IEnumerable<ServiceUserResponse> serviceUserResponses = _mapper
      .Map<IEnumerable<ServiceUserResponse>>(serviceUsers);

    return Ok(serviceUserResponses);
  }

  [HttpGet, Route("ReferralStatusReason")]
  [MapToApiVersion("2.0")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<ActionResult<IEnumerable<GetReferralStatusReasonsResponse>>>
    GetReferralStatusReasons([FromQuery] string group)
  {
    ReferralStatusReason[] referralStatusReasons;

    if (string.IsNullOrWhiteSpace(group))
    {
      referralStatusReasons = await Service.GetReferralStatusReasonsAsync();
    }
    else
    {
      bool isValidProviderReferralStatusReasonGroup = false;
      if (group.TryParseToEnumName(out ReferralStatusReasonGroup filter))
      {
        isValidProviderReferralStatusReasonGroup = 
          ReferralStatusReasonGroupConstants.ProviderStatuses.HasFlag(filter);
      }

      if (!isValidProviderReferralStatusReasonGroup)
      {
        return Problem($"Unknown referral status reason group: '{group}'.",
          statusCode: StatusCodes.Status400BadRequest);
      }

      referralStatusReasons = await Service
        .GetReferralStatusReasonsByGroupAsync(filter);
    }

    List<GetReferralStatusReasonsResponse> response = referralStatusReasons
      .Select(x => new GetReferralStatusReasonsResponse(
        x.Description,
        x.Groups,
        x.Id))
      .ToList();

    return Ok(response);
  }

  /// <summary>
  /// Used to update the provider’s service user details including the 
  /// service user’s programme start date, their self-reported weight, 
  /// weekly engagement measure, coaching time for level’s 2 and 3 and 
  /// their programme outcome
  /// </summary>
  /// <param name="requests"></param>
  /// <returns></returns>
  [HttpPut]
  [MapToApiVersion("1.0")]
  [Consumes(MediaTypeNames.Application.Json)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Put(
    IEnumerable<ServiceUserSubmissionRequest> requests)
  {
    try
    {
      return await UpdateReferralAsync(requests);
    }
    catch (Exception ex)
    {
      LogException(ex, requests);
      return Problem(statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  [HttpPut]
  [MapToApiVersion("2.0")]
  [Consumes(MediaTypeNames.Application.Json)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> PutV2(
    IEnumerable<ServiceUserSubmissionRequest> requests)
  {
    try
    {
      ServiceUserSubmissionRequestV2[] models =
        _mapper.Map<ServiceUserSubmissionRequestV2[]>(requests);

      ReferralStatusReason[] reasonList =
        await Service.GetReferralStatusReasonsAsync();

      models.Select(t =>
      {
        t.ReasonList = reasonList;
        return t;
      }).ToList();

      return await UpdateReferralAsync(models);
    }
    catch (Exception ex)
    {
      LogException(ex, requests);
      return Problem(statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  private async Task<IActionResult> UpdateReferralAsync(
    IEnumerable<IServiceUserSubmissionRequest> requests)
  {
    IEnumerable<ServiceUserSubmissionResponse> responses =
      await Service.ProviderSubmissionsAsync(requests);

    if (!responses.Any(r => r.ResponseStatus == StatusType.Invalid))
    {
      return Ok();
    }

    ValidationProblemDetails problemDetails =
      new()
      {
        Status = StatusCodes.Status400BadRequest,
        Title = "One or more validation errors occurred."
      };

    for (int i = 0; i < responses.Count(); i++)
    {
      ServiceUserSubmissionResponse response = responses.ElementAt(i);

      if (response.ResponseStatus != StatusType.Invalid)
      {
        continue;
      }

      LogInformation(response.GetErrorMessage());
      problemDetails.Errors.Add($"[{i}]", response.Errors.ToArray());

    }

    return new ObjectResult(problemDetails);
  }

  private ProviderService Service
  {
    get
    {
      ProviderService service = _service as ProviderService;
      service.User = User;
      return service;
    }
  }
}
