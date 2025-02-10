using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Referral.Api.Models;

namespace WmsHub.Referral.Api.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/[Controller]")]
  [Route("[Controller]")]
  public class StaffReferralController 
    : BaseReferralController<StaffReferralOptions>
  {
    private readonly IMapper _mapper;

    protected override int MaxActiveAccessKeys => _options.MaxActiveAccessKeys;

    public StaffReferralController(
      ILogger logger,
      IMapper mapper,
      IOptions<StaffReferralOptions> options,
      IReferralService referralService)
      : base(
        logger.ForContext<StaffReferralController>(),
        options.Value,
        referralService)
    {
      _mapper = mapper;
    }

    /// <summary>
    /// Checks if the provided email address is already in use
    /// </summary>
    /// <param name="emailInUse">An object containing the email to check</param>
    /// <response code="200">Email is not in use</response>
    /// <response code="400">Missing/invalid values</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="409">Email is in use</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Route("EmailInUse")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest,
      Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Consumes("application/json")]
    public async Task<IActionResult> IsEmailInUse(
      [FromBody] SelfReferralEmailInUse emailInUse)
    {
      try
      {
        if (User != null 
          && User.FindFirst(ClaimTypes.Name).Value != "SelfReferral.Service")
        {
          return BaseReturnResponse(
            StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        ValidateEmailDomain(emailInUse.Email);

        await Service.CheckSelfReferralIsUniqueAsync(emailInUse.Email);

        return Ok();
      }
      catch (Exception ex)
      {
        if (ex is ReferralNotUniqueException)
        {
          Log.Debug(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status409Conflict);
        }
        if (ex is SelfReferralValidationException)
        {
          LogInformation(ex.Message);
          return ValidationProblem(new ValidationProblemDetails(
            (ex as SelfReferralValidationException).ValidationResults));
        }
        else if (ex is EmailWhiteListException)
        {
          Log.Debug(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status400BadRequest);
        }
        else
        {
          LogException(ex, emailInUse);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    /// <summary>
    /// Creates a staff-referral
    /// </summary>
    /// <param name="selfReferralPostRequest"></param>
    /// <returns>A newly created referral</returns>
    /// <response code="200">Referral created</response>
    /// <response code="400">Missing/invalid values</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="409">Referral already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest,
      Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Consumes("application/json")]
    public async Task<IActionResult> Post([FromBody] SelfReferralPostRequest
      selfReferralPostRequest)
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "SelfReferral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        selfReferralPostRequest.InjectionRemover();

        ValidateEmailDomain(selfReferralPostRequest.Email);

        ISelfReferralCreate selfReferralCreate =
          _mapper.Map<SelfReferralCreate>(selfReferralPostRequest);

        IReferral referral = await Service
          .CreateSelfReferral(selfReferralCreate);

        IReferralPostResponse response =
          await Service.GetReferralCreateResponseAsync(referral);

        return Ok(response);
      }
      catch (Exception ex)
      {
        if (ex is SelfReferralValidationException)
        {
          LogInformation(ex.Message);
          return ValidationProblem(new ValidationProblemDetails(
            (ex as SelfReferralValidationException).ValidationResults));
        }
        else if (ex is ReferralNotUniqueException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status409Conflict);
        }
        else if (ex is NoProviderChoicesFoundException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status204NoContent);
        }
        else if (ex is EmailWhiteListException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status400BadRequest);
        }
        else
        {
          LogException(ex, selfReferralPostRequest);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest,
      Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Consumes("application/json")]
    public async Task<IActionResult> Put(
      [FromBody] SelfReferralPutRequest selfReferralUpdateRequest)
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "SelfReferral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        selfReferralUpdateRequest.InjectionRemover();

        await Service.UpdateReferralWithProviderAsync(
          selfReferralUpdateRequest.Id,
          selfReferralUpdateRequest.ProviderId);

        return Ok();
      }
      catch (Exception ex)
      {
        if (ex is SelfReferralValidationException)
        {
          LogInformation(ex.Message);
          return ValidationProblem(new ValidationProblemDetails(
            (ex as SelfReferralValidationException).ValidationResults));
        }
        else if (ex is ReferralProviderSelectedException ||
          ex is ReferralInvalidStatusException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status409Conflict);
        }
        else if (ex is ProviderSelectionMismatch ||
          ex is ReferralNotFoundException)
        {
          LogInformation(ex.Message);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status400BadRequest);
        }
        else
        {
          LogException(ex, selfReferralUpdateRequest);
          return Problem(ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
    }

    /// <summary>
    /// Get List of Ethnicities
    /// </summary>
    /// <response code="200">Returns the list of ethnicities</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [Route("Ethnicity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEthnicities()
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "SelfReferral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        IEnumerable<Models.Ethnicity> ethnicities =
          _mapper.Map<IEnumerable<Models.Ethnicity>>(
            await Service.GetEthnicitiesAsync(ReferralSource.SelfReferral));

        return Ok(ethnicities);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
      }
    }


    /// <summary>
    /// Get List of staff roles
    /// </summary>
    /// <response code="200">Returns the list of staff roles</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="500">Internal server error</response>/// 
    [HttpGet]
    [Route("StaffRole")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStaffRoles()
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "SelfReferral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        IEnumerable<Models.StaffRole> staffRoles =
          _mapper.Map<IEnumerable<Models.StaffRole>>(
            await Service.GetStaffRolesAsync());

        return Ok(staffRoles);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
      }
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
      
      return await GenerateAccessKey(AccessKeyType.StaffReferral,
        email,
        expireMinutes);
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
    /// <response code="500">
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
      return await ValidateAccessKey(AccessKeyType.StaffReferral,
        email, keyCode);
    }

    protected void ValidateEmailDomain(string email)
    {
      if (_options.IsEmailDomainWhitelistEnabled && 
        !_options.IsEmailDomainInWhitelist(email))
      {
        throw new EmailWhiteListException(
          $"Email address {email} contains a domain " +
          $"that is not in the white list.");
      }
    }
  }
}