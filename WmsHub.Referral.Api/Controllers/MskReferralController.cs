using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Models.ReferralService.MskReferral;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Extensions;
using WmsHub.Referral.Api.Models;
using WmsHub.Referral.Api.Models.MskReferral;

namespace WmsHub.Referral.Api.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Authorize(Policy = AuthPolicies.Msk.POLICY_NAME)]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[Controller]")]
  public class MskReferralController : BaseController
  {
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly MskReferralOptions _options;

    public MskReferralController(
      ILogger logger,
      IMapper mapper,
      IOptions<MskReferralOptions> options,
      IReferralService referralService)
      : base(referralService)
    {
      _logger = logger.ForContext<MskReferralController>();
      _mapper = mapper;
      _options = options.Value;
    }

    /// <summary>
    /// Get a status code which determines if the NHS number can be used
    /// to create a MSK referral. A 204 means a referral can be created with
    /// this NHS number. A 409 means a referral cannot be created with this 
    /// NHS number. </summary>
    /// <remarks>
    /// A 204 response will be returned if the provided NHS number:
    /// <ul><li>Does not match a referral.</li>
    /// <li>Matches a referral which is currently cancelled and a provider was 
    /// not selected.</li></ul>
    /// A 409 response will be returned if the provided NHS number:
    /// <ul><li>Matches a referral that is currently being processed.</li>
    /// <li>Matches a referral which is currently cancelled and a provider was
    /// selected.</li></ul>
    /// </remarks>
    /// <param name="nhsNumber">A valid NHS Number.</param>
    /// <response code="204">A referral can be created with this NHS number.
    /// </response>
    /// <response code="400">NHS Number is invalid.</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="403">Invalid authorization</response>
    /// <response code="409">A referral cannot be created with this NHS number.
    /// </response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [Route("NhsNumber/{nhsNumber:minlength(10):maxlength(10)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IsNhsNumberInUse(string nhsNumber)
    {
      IsNhsNumberInUseRequest request = null;
      try
      {
        request = new() { NhsNumber = nhsNumber };
        ValidationModel validationResult = request.IsValid();

        if (!validationResult.IsValid)
        {
          _logger.Debug(validationResult.Error);
          return Problem(validationResult.Error,
            statusCode: StatusCodes.Status400BadRequest);
        }

        InUseResponse inUseResponse = await Service
          .IsNhsNumberInUseAsync(request.NhsNumber);

        // Existing referral with same NHS number?
        if (inUseResponse.InUseResult.HasFlag(InUseResult.NotFound))
        {
          // No, Can create a referral with this NHS number
          return NoContent();
        }
        // Yes, Is referral cancelled?
        else if (inUseResponse.InUseResult.HasFlag(InUseResult.Cancelled))
        {
          // Yes, Was provider selected?
          if (inUseResponse.InUseResult.HasFlag(InUseResult.ProviderSelected))
          {
            // Yes, Cannot create a referral with this NHS number
            _logger.Debug("NHS number {nhsNumber} was previously used " +
              "with a referral that had selected a provider.",
              nhsNumber);
            return Problem($"NHS number {nhsNumber} was previously used " +
              $"with a referral that had selected a provider.",
              statusCode: StatusCodes.Status409Conflict);
          }
          else
          {
            // No, Can create a referral with this NHS number
            return NoContent();
          }
        }
        else
        {
          // Not cancelled, Cannot create a referral with this NHS number
          _logger.Debug("NHS number {nhsNumber} is already in use.",
            nhsNumber);
          return Problem($"NHS number {nhsNumber} is already in use.",
            statusCode: StatusCodes.Status409Conflict);
        }
      }
      catch (Exception ex)
      {
        LogException(ex, request);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    /// <summary>
    /// Get List of Ethnicities
    /// </summary>
    /// <response code="200">Returns the list of ethnicities</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="401">Invalid authorization</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Route("Ethnicity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEthnicities()
    {
      try
      {
        IEnumerable<Models.Ethnicity> ethnicities =
          _mapper.Map<IEnumerable<Models.Ethnicity>>(
            await Service.GetEthnicitiesAsync(ReferralSource.Msk));

        return Ok(ethnicities);
      }
      catch (Exception ex)
      {
        _logger.Error(ex, "Failed to get ethnicities");
        return Problem(statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    /// <summary>
    /// Get List of MSK Hubs for the pilot
    /// </summary>
    /// <response code="200">Returns the list of ethnicities</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="401">Invalid authorization</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Route("MskHub")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPilotMskHubs()
    {
      try
      {
        if (_options.WhitelistHasValues)
        {
          return Ok(_options.MskHubs
            .OrderBy(m => m.Value)
            .Select(m => new MskHub(m.Value, m.Key))
            .ToArray());
        }
        else
        {
          throw new Exception("MSK options contains zero MSK Hubs.");
        }
      }
      catch (Exception ex)
      {
        _logger.Error(ex, "Failed to get MSK hubs.");
        return Problem(statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    /// <summary>
    /// Creates a MSK referral
    /// </summary>
    /// <param name="request">A PostRequest object containing the properties
    /// with which to create an MSK referral.</param>
    /// <response code="204">The referral was successfully created.</response>
    /// <response code="400">The PostRequest object contains invalid 
    /// properties.</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="403">Invalid authorization</response>
    /// <response code="409">A referral cannot be created with the supplied
    /// NHS number.</response>
    /// <response code="500">Internal server error.</response>/// 
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Consumes("application/json")]
    public async Task<IActionResult> Post([FromBody] PostRequest request)
    {
      try
      {
        if (!IsMskOdsCodeInWhitelist(request.ReferringMskHubOdsCode))
        {
          Log.Debug(
            "ReferringMskHubOdsCode {odsCode} is not in whitelist.",
            request.ReferringMskHubOdsCode);

          return Problem(
            detail: $"The ReferringMskHubOdsCode is not in the whitelist.",
            statusCode: StatusCodes.Status400BadRequest);
        }

        IMskReferralCreate create = _mapper.Map<MskReferralCreate>(request);

        await Service.CreateMskReferralAsync(create);

        return NoContent();
      }
      catch (Exception ex)
      {
        if (ex is MskReferralValidationException)
        {
          LogInformation(ex.Message);
          return ValidationProblem(new ValidationProblemDetails(
            (ex as MskReferralValidationException).ValidationResults));
        }
        else if (ex is ReferralNotUniqueException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status409Conflict);
        }
        else
        {
          LogException(ex, request);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    private bool IsMskOdsCodeInWhitelist(string referringMskHubOdsCode)
    {
      bool isMskOdsCodeInWhitelist = false;

      if (_options.IsWhitelistEnabled)
      {
        if (_options.WhitelistHasValues)
        {
          isMskOdsCodeInWhitelist = _options.MskHubs
            .ContainsKey(referringMskHubOdsCode);
        }
      }
      else
      {
        isMskOdsCodeInWhitelist = true;
      }

      return isMskOdsCodeInWhitelist;
    }

    private IReferralService Service
    {
      get
      {
        IReferralService service = _service as IReferralService;
        service.User = User;
        return service;
      }
    }
  }
}
