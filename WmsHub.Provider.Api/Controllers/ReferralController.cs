using Asp.Versioning;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Api.Controllers;
using WmsHub.Provider.Api.Models;

namespace WmsHub.Provider.Api.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/[controller]")]
[Route("[controller]")]
public class ReferralController : BaseController
{
  private readonly IMapper _mapper;

  public ReferralController(
    IProviderService providerService,
    IMapper mapper)
    : base(providerService)
  {
    _mapper = mapper;
  }

  /// <summary>
  /// Returns status and submissions for a given referral's UBRN.
  /// </summary>
  /// <response code="200">Status and submissions returned</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="403">Forbidden</response>
  /// <response code="404">Resource not found</response>
  /// <response code="500">Internal server error</response>
  /// <response code="503">Service unavailable, please try again</response> 
  /// <returns>The status of the referral and its submissions</returns>
  [HttpGet("{ubrn:length(12)}")]
  [MapToApiVersion("1.0")]
  [MapToApiVersion("2.0")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<ActionResult<ReferralResponse>> Get(string ubrn)
  {
    ReferralResponse referralResponse = _mapper.Map<ReferralResponse>(
      await Service.GetReferralStatusAndSubmissions(ubrn));

    if (referralResponse == null)
    {
      return Problem(
        detail: $"Unable to find a referral with a UBRN of {ubrn}.",
        statusCode: StatusCodes.Status404NotFound);
    }
    else
    {
      return Ok(referralResponse);
    }
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
