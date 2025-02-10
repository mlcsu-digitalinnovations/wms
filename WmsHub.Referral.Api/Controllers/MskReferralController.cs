using Asp.Versioning;
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
using WmsHub.Business.Models;
using WmsHub.Business.Models.ReferralService.MskReferral;
using WmsHub.Business.Services;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Referral.Api.Models;
using WmsHub.Referral.Api.Models.MskReferral;

namespace WmsHub.Referral.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Authorize(Policy = AuthPolicies.Msk.POLICY_NAME)]
[Route("v{version:apiVersion}/[controller]")]
[Route("[Controller]")]
public class MskReferralController 
  : BaseReferralController<MskReferralOptions>
{
  private readonly IMapper _mapper;
  private readonly IMskOrganisationService _mskOrganisationService;

  protected override int MaxActiveAccessKeys => _options.MaxActiveAccessKeys;

  public MskReferralController(
    ILogger logger,
    IMapper mapper,
    IMskOrganisationService mskOrganisationService,
    IOptions<MskReferralOptions> options,
    IReferralService referralService)
    : base(
      logger.ForContext<MskReferralController>(),
      options.Value,
      referralService)
  {
    _mapper = mapper;
    _mskOrganisationService = mskOrganisationService;
  }

  /// <summary>
  /// Generates an access key.
  /// </summary>
  /// <param name="email">
  /// The email address to associated with the key.
  /// </param>
  /// <param name="expireMinutes">
  /// [optional] The number of minutes until the key expires, defaults to 10.
  /// </param>
  /// <returns>
  /// An object containing the key code for the email and its expiry date and 
  /// time.
  /// </returns>
  /// <response code="200">
  /// Success
  /// </response>
  /// <response code="400">
  /// There was a validation error with the provided email or expire minutes.
  /// </response>
  /// <response code="401">
  /// An invalid API key was provided in the header.
  /// </response>
  /// <response code="403">
  /// The provided email has a domain that is not in the domain whitelist.
  /// </response>
  /// <response code="500">
  /// Internal server error.
  /// </response>
  [HttpGet]
  [Route("GenerateKey")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GenerateAccessKey(
    string email,
    int expireMinutes = 10)
  {
    return await GenerateAccessKey(AccessKeyType.MskReferral,
      email, expireMinutes);
  }

  /// <summary>
  /// Get List of Ethnicities
  /// </summary>
  /// <response code="200">Returns the list of ethnicities</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="403">Invalid authorization</response>
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
      _log.Error(ex, "Failed to get ethnicities");
      return Problem(statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  /// <summary>
  /// Get List of MSK Hubs for the pilot
  /// </summary>
  /// <response code="200">Returns the list of MSK Hubs</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="401">Invalid authorization</response>
  /// <response code="500">Internal server error</response>
  [HttpGet]
  [Route("MskHub")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetMskHubs()
  {
    try
    {
      IEnumerable<MskOrganisation> mskOrganisations = 
        await _mskOrganisationService.GetAsync();

      return Ok(mskOrganisations
        .Select(m => new MskHub(m.SiteName, m.OdsCode))
        .ToArray());
    }
    catch (Exception ex)
    {
      _log.Error(ex, "Failed to get MSK hubs.");
      return Problem(statusCode: StatusCodes.Status500InternalServerError);
    }
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
        _log.Debug(validationResult.Error);
        return Problem(validationResult.Error,
          statusCode: StatusCodes.Status400BadRequest);
      }

      try
      {
        await Service.CheckReferralCanBeCreatedWithNhsNumberAsync(
          request.NhsNumber);
      }
      catch (ReferralNotUniqueException ex)
      {
        Log.Debug(ex.Message, request.NhsNumber);
        return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict);
      }
      catch (InvalidOperationException ex)
      {
        Log.Debug(ex.Message, request.NhsNumber);
        return Problem(ex.Message, statusCode: StatusCodes.Status409Conflict);
      }

      return NoContent();
    }
    catch (Exception ex)
    {
      LogException(ex, request);
      return Problem(ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
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
  /// <response code="500">Internal server error.</response> 
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
      if (!await IsMskOdsCodeInWhitelist(request.ReferringMskHubOdsCode))
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
      else if (ex is EmailWhiteListException)
      {
        LogInformation(ex.Message);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status400BadRequest);
      }
      else
      {
        LogException(ex, request);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }
  }


  /// <summary>
  /// Validates an access key for the provided email address.
  /// </summary>
  /// <param name="email">
  /// The email address associated with the key.
  /// </param>
  /// <param name="keyCode">
  /// The key code associated with the email address.
  /// </param>
  /// <returns>
  /// An object containing the validation details of the key and email.
  /// </returns>
  /// <response code="200">
  /// Success
  /// </response>
  /// <response code="400">
  /// There was a validation error with the provided email or key code.
  /// </response>
  /// <response code="401">
  /// An invalid API key was provided in the header.
  /// </response>
  /// <response code="403">
  /// The provided email has a domain that is not in the domain whitelist.
  /// </response>
  /// <response code="404">
  /// The provided email was not found.
  /// </response>
  /// <response code="422">
  /// The key code has a problem which is described by the detail and type
  /// properties of the returned problem details object. Type can be: Expired,
  /// Incorrect or TooManyAttempts.
  /// </response>  
  /// /// <response code="500">
  /// Internal server error.
  /// </response>
  [HttpGet]
  [Route("ValidateKey")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ValidateAccessKey(
    string email,
    string keyCode)
  {
    return await ValidateAccessKey(AccessKeyType.MskReferral, email, keyCode);
  }

  private async Task<bool> IsMskOdsCodeInWhitelist(
    string referringMskHubOdsCode)
  {
    if (_options.IsMskHubWhitelistEnabled)
    {
      return await _mskOrganisationService.ExistsAsync(referringMskHubOdsCode);
    }
    else
    {
      return true;
    }
  }
}
