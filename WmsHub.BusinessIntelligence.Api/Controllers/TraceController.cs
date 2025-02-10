
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.BusinessIntelligence;
using WmsHub.Business.Services.Interfaces;
using WmsHub.BusinessIntelligence.Api.Models;
using WmsHub.Common.Api.Controllers;

namespace WmsHub.BusinessIntelligence.Api.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [Authorize(Policy = AuthPolicies.DefaultAuthPolicy.POLICYNAME)]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[Controller]")]
  public class TraceController : BaseController
  {
    private readonly IMapper _mapper;
    private BusinessIntelligenceOptions _options;

    public TraceController(
      IBusinessIntelligenceService busIntelliService,
      IMapper mapper,
      IOptions<BusinessIntelligenceOptions> options)
      : base(busIntelliService)
    {
      _mapper = mapper;
      _options = options.Value;
    }

    /// <summary>
    /// Lists all service users awaiting NHS number tracing.
    /// </summary>
    /// <response code="200">Request successful.</response>
    /// <response code="204">No data returned.</response>
    /// <response code="401">Invalid authentication.</response>
    /// <response code="500">Internal server error.</response>
    /// <response code="503">Service unavailable, please try again.</response> 
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IEnumerable<Models.NhsNumberTrace>>> Get()
    {
      try
      {

        if (_options.IsTraceIpWhitelistEnabled)
        {
          CheckAzureSocketIpAddressInWhitelist(_options.TraceIpWhitelist);
        }

        IEnumerable<Business.Models.NhsNumberTrace> businessObjects =
          await Service.GetUntracedNhsNumbers();

        if (!businessObjects.Any())
        {
          return NoContent();
        }

        IEnumerable<Models.NhsNumberTrace> nhsNumbersToTrace =
          _mapper.Map<IEnumerable<Models.NhsNumberTrace>>(businessObjects);

        return Ok(nhsNumbersToTrace);
      }
      catch (Exception ex)
      {
        LogException(ex);

        if (ex is UnauthorizedAccessException)
        {
          return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }
        else
        {
          return Problem(ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    /// <summary>
    /// Post an array of traced service users.
    /// </summary>
    /// <remarks>
    /// If the NHS Number cannot be traced submit null for the NHS Number.
    /// If the GP Practice Ods Code is Unknown submit "V81999".
    /// If the GP Practice Name is Unknown submit "Unknown".
    /// </remarks>
    /// <response code="200">Request successful.</response>
    /// <response code="400">One or more submitted traces have an error.
    /// </response>
    /// <response code="401">Invalid authentication.</response>
    /// <response code="500">Internal server error.</response>
    /// <response code="503">Service unavailable, please try again.</response> 
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [Produces("application/json")]
    public async Task<IActionResult> Post(
      [FromBody] IEnumerable<Models.SpineTraceResult> spineTraceResults)
    {
      try
      {
        if (_options.IsTraceIpWhitelistEnabled)
        {
          CheckAzureSocketIpAddressInWhitelist(_options.TraceIpWhitelist);
        }

        IEnumerable<Business.Models.SpineTraceResult> businessObjects = _mapper
          .Map<IEnumerable<Business.Models.SpineTraceResult>>(
          spineTraceResults);

        List<SpineTraceResponse> responses =
          await Service.UpdateSpineTraced(businessObjects);

        dynamic response = new ExpandoObject();

        if (responses.Any(t => t.Errors.Any()))
        {
          response.SuccessCount = responses.Count(t => !t.Errors.Any());
          response.FailureCount = responses.Count(t => t.Errors.Any());
          response.TraceResultsWithErrors =
            responses.Where(t => t.Errors.Any()).ToArray();
        }

        return Ok(response);
      }
      catch (Exception ex)
      {
        LogException(ex);

        if (ex is ReferralNotFoundException
          || ex is NhsNumberTraceMismatchException
          || ex is ValidationException)
        {
          return Problem(
            ex.Message,
            statusCode: StatusCodes.Status400BadRequest);
        }
        else if (ex is UnauthorizedAccessException)
        {
          return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }
        else
        {
          return Problem(
            ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    /// <summary>
    /// Get a list of referrals and their referring practice ODS code where a 
    /// referral's status has been set to DischargeAwaitingTrace more than 
    /// once.
    /// </summary>
    /// <response code="200">Request successful.</response>
    /// <response code="204">No data returned.</response>
    /// <response code="401">Invalid authentication.</response>
    /// <response code="500">Internal server error.</response>
    /// <response code="503">Service unavailable, please try again.</response> 

    [HttpGet("traceissues")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IEnumerable<Models.TraceIssueReferral>>>
      GetTraceIssues()
    {
      try
      {
        if (_options.IsTraceIpWhitelistEnabled)
        {
          CheckAzureSocketIpAddressInWhitelist(_options.TraceIpWhitelist);
        }

        IEnumerable<Business.Models.Tracing.TraceIssueReferral> businessObjects
          = await Service.GetTraceIssueReferralsAsync();

        if (!businessObjects.Any())
        {
          return NoContent();
        }

        IEnumerable<TraceIssueReferral> referrals =
          _mapper.Map<IEnumerable<TraceIssueReferral>>(businessObjects);

        return Ok(referrals);
      }
      catch (Exception ex)
      {
        LogException(ex);

        if (ex is UnauthorizedAccessException)
        {
          return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }
        else
        {
          return Problem(ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
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
}

