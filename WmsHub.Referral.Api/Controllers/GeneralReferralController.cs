using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Models.GeneralReferral;

namespace WmsHub.Referral.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Authorize(Policy = AuthPolicies.GeneralReferral.POLICY_NAME)]
[Route("v{version:apiVersion}/[controller]")]
[Route("[Controller]")]
public class GeneralReferralController : BaseController
{
  private readonly IMapper _mapper;

  public GeneralReferralController(
    IReferralService referralService,
    IMapper mapper)
    : base(referralService)
  {
    _mapper = mapper;
  }

  /// <summary>
  /// Cancels an elective care referral
  /// </summary>
  /// <param name="request">GeneralReferralPutRequest</param>
  /// <param name="id">Referral Id</param>
  /// <response code="200">Referral cancelled.</response>
  /// <response code="400">Missing or invalid values.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="404">Referral not found.</response>
  /// <response code="409">Referral not in valid state.</response>
  /// <response code="500">Internal server error</response>
  [HttpPut("{id:guid}/cancel")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Cancel(
    [FromBody] GeneralReferralCancelRequest request, 
    Guid id)
  {
    try
    {
      request.InjectionRemover();

      IGeneralReferralCancel cancellation = _mapper
        .Map<GeneralReferralCancel>(request);

      cancellation.Id = id;

      await Service.CancelGeneralReferralAsync(cancellation);

      return Ok();

    }
    catch (Exception ex)
    {
      if (ex is GeneralReferralValidationException)
      {
        LogInformation(ex.Message);
        return ValidationProblem(new ValidationProblemDetails(
          (ex as GeneralReferralValidationException).ValidationResults));
      }
      else if (ex is ReferralNotFoundException)
      {
        LogInformation(ex.Message);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status404NotFound);
      }
      else if (ex is ReferralInvalidReferralSourceException
        || ex is ReferralInvalidStatusException)
      {
        LogInformation(ex.Message);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status409Conflict);
      }
      else if (ex is EthnicityNotFoundException)
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
  /// Get a list of Ethnicities
  /// </summary>
  /// <response code="200">Returns the list of ethnicities</response>
  /// <response code="401">Invalid authentication</response>
  /// <response code="500">Internal server error</response>
  [HttpGet]
  [Route("Ethnicity")]
  [ProducesResponseType(StatusCodes.Status200OK,
    Type = typeof(IEnumerable<Models.Ethnicity>))]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [Produces("application/json")]
  public async Task<IActionResult> GetEthnicities()
  {
    try
    {
      IEnumerable<Models.Ethnicity> ethnicities =
        _mapper.Map<IEnumerable<Models.Ethnicity>>(
          await Service.GetEthnicitiesAsync(ReferralSource.SelfReferral));

      return Ok(ethnicities);
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  /// <summary>
  /// Returns a public or elective care referral that matches the provided NHS 
  /// number if its status is such that it can be updated and a provider 
  /// selected.
  /// </summary>
  /// <remarks>
  /// A 200 response with the associated referral object will be returned if 
  /// the provided NHS number:
  /// <ul><li>Matches a public or elective care referral which has not had a 
  /// provider selected.</li></ul>
  /// A 204 response will be returned if the provided NHS number:
  /// <ul><li>Does not match an existing referral.</li>
  /// <li>Matches an existing referral that has been completed and is eligible 
  /// for another referral.</li>
  /// A 409 response will be returned if the provided NHS number:
  /// <li>Matches a referral which is active but is not a public or elective 
  /// care referral.</li>
  /// <li>Matches a public or elective care referral that has had a provider 
  /// selected.</li>
  /// <li>Matches an existing referral that has been completed and is not yet 
  /// eligible for another referral.</li>
  /// </ul>
  /// </remarks>
  /// <response code="200">Request successful, updateable referral returned.
  /// </response>
  /// <response code="204">Request successful and a new referral can be 
  /// created with this NHS number.</response>
  /// <response code="400">NHS Number is invalid.</response>
  /// <response code="401">Unauthorised</response>
  /// <response code="403">Forbidden</response>
  /// <response code="409">A referral exists that cannot be 
  /// updated and a new referral cannot be created with this NHS number.
  /// </response>
  /// <response code="500">Internal server error.</response>
  [HttpGet("{nhsNumber:length(10)}")]
  [ProducesResponseType(StatusCodes.Status200OK,
    Type = typeof(GetNhsNumberOkResponse))]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest,
    Type = typeof(ValidationProblemDetails))]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status409Conflict,
    Type = typeof(GetNhsNumberConflictResponse))]
  [ProducesResponseType(StatusCodes.Status500InternalServerError,
    Type = typeof(ProblemDetails))]
  [Produces("application/json")]
  public async Task<IActionResult> GetNhsNumber(
    [FromRoute] GetNhsNumberRequest request)
  {
    try
    {
      CanCreateReferralResponse response = await Service
        .CanGeneralReferralBeCreatedWithNhsNumberAsync(request.NhsNumber);

      switch (response.CanCreateResult)
      {
        case CanCreateReferralResult.CanCreate:
          return NoContent();

        case CanCreateReferralResult.ProgrammeStarted:
        case CanCreateReferralResult.ProviderSelected:
        case CanCreateReferralResult.IneligibleReferralSource:
          return Conflict(new GetNhsNumberConflictResponse(response));

        case CanCreateReferralResult.UpdateExisting:
          return Ok(_mapper.Map<GetNhsNumberOkResponse>(response.Referral));

        default:
          Log.Error(
            $"Unhandled {nameof(CanCreateReferralResponse)} of {{response}}.",
            response.CanCreateResult);
          return Problem("Unable to get NHS number.",
            statusCode: StatusCodes.Status500InternalServerError);
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
  /// Creates a public general referral.
  /// </summary>
  /// <remarks>If the request is successful a list of available providers
  /// will be returned along with the id of the new referral.</remarks>
  /// <param name="request">GeneralReferralPostRequest</param>
  /// <response code="200">Request successful, referral created and a list of 
  /// providers has been returned.</response>
  /// <response code="400">Missing or invalid values.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="409">Referral already exists.</response>
  /// <response code="500">Internal server error.</response>
  [HttpPost]
  [ProducesResponseType(StatusCodes.Status200OK,
    Type = typeof(IReferralPostResponse))]
  [ProducesResponseType(StatusCodes.Status400BadRequest,
    Type = typeof(ValidationProblemDetails))]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [Consumes("application/json")]
  public async Task<IActionResult> Post([FromBody] PostRequest request)
  {
    try
    {
      request.InjectionRemover();

      IGeneralReferralCreate referralCreate =
        _mapper.Map<GeneralReferralCreate>(request);

      IReferral referral = await Service
        .CreateGeneralReferral(referralCreate);

      IReferralPostResponse response = await Service
        .GetReferralCreateResponseAsync(referral);

      return Ok(response);
    }
    catch (Exception ex)
    {
      if (ex is GeneralReferralValidationException)
      {
        LogInformation(ex.Message);
        return ValidationProblem(new ValidationProblemDetails(
          (ex as GeneralReferralValidationException).ValidationResults));
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
      else
      {
        LogException(ex, request);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }
  }

  /// <summary>
  /// Updates a general referral
  /// </summary>
  /// <param name="request">GeneralReferralPutRequest</param>
  /// <param name="id">Referral Id</param>
  /// <returns>A Update referral with selection of providers</returns>
  /// <response code="200">Referral Updated.</response>
  /// <response code="400">Missing or invalid values.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="409">Referral already exists.</response>
  /// <response code="500">Internal server error</response>
  [HttpPut("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status200OK,
    Type = typeof(IReferralPostResponse))]
  [ProducesResponseType(StatusCodes.Status400BadRequest,
    Type = typeof(ValidationProblemDetails))]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Put(
    [FromBody] PutRequest request, Guid id)
  {
    try
    {
      request.InjectionRemover();

      IGeneralReferralUpdate update = _mapper
        .Map<GeneralReferralUpdate>(request);

      update.Id = id;

      ValidationModel validationResult = update.IsValid();
      if (!validationResult.IsValid)
      {
        LogInformation(validationResult.Error);
        return Problem(validationResult.Error,
          statusCode: StatusCodes.Status400BadRequest);
      }

      IReferral referral = await Service.UpdateGeneralReferral(update);

      IReferralPostResponse response =
        await Service.GetReferralCreateResponseAsync(referral);

      return Ok(response);

    }
    catch (Exception ex)
    {
      if (ex is GeneralReferralValidationException)
      {
        LogInformation(ex.Message);
        return ValidationProblem(new ValidationProblemDetails(
          (ex as GeneralReferralValidationException).ValidationResults));
      }
      else if (ex is ReferralNotFoundException)
      {
        LogInformation(ex.Message);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status404NotFound);
      }
      else if (ex is NhsNumberUpdateReferralMismatchException)
      {
        LogInformation(ex.Message);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status409Conflict);
      }
      else if (ex is ReferralInvalidStatusException)
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
      else
      {
        LogException(ex, request);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }
  }


  /// <summary>
  /// Adds a provider to a referral
  /// </summary>
  /// <param name="referralId">The Id of the referral to update with the 
  /// provide.r</param>
  /// <param name="providerId">The Id of the provider to update the referral
  /// with.</param>
  /// <returns></returns>
  /// <response code="200">Provider added to referral.</response>
  /// <response code="400">Missing or invalid values.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="409">Provider already exists.</response>
  /// <response code="500">Internal server error.</response>
  [HttpPut]
  [Route("{referralId:guid}/Provider/{providerId:guid}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest,
    Type = typeof(ValidationProblemDetails))]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  [Consumes("application/json")]
  public async Task<IActionResult> PutProvider(
    Guid referralId,
    Guid providerId)
  {
    PutProviderRequest request = new()
    {
      Id = referralId,
      ProviderId = providerId
    };

    try
    {
      ValidationModel validationResult = request.IsValid();
      if (!validationResult.IsValid)
      {
        LogInformation(validationResult.Error);
        return Problem(validationResult.Error,
          statusCode: StatusCodes.Status400BadRequest);
      }

      _ = await Service.UpdateReferralWithProviderAsync(request.Id,
        request.ProviderId,
        ReferralSource.GeneralReferral);

      return Ok();
    }
    catch (Exception ex)
    {
      if (ex is GeneralReferralValidationException)
      {
        LogInformation(ex.Message);
        return ValidationProblem(new ValidationProblemDetails(
          (ex as GeneralReferralValidationException).ValidationResults));
      }
      else if (ex is ReferralProviderSelectedException or
               ReferralInvalidStatusException)
      {
        LogInformation(ex.Message);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status409Conflict);
      }
      else if (ex is ProviderSelectionMismatch or
               ReferralNotFoundException)
      {
        LogInformation(ex.Message);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status409Conflict);
      }
      else if (ex is ReferralNotFoundException)
      {
        LogInformation(ex.Message);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status404NotFound);
      }
      else if (ex is ReferralUpdateException)
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

  /// <summary>
  /// Validates the link ID of a user attempting to access the EC UI.
  /// </summary>
  /// <response code="200">The link ID matches an Elective Care referral text message.</response>
  /// <response code="204">The link ID does not match any Elective Care referral text messages.
  /// </response>
  /// <response code="400">The link ID is not a valid link ID.</response>
  /// <response code="401">Invalid authentication.</response>
  /// <response code="403">Invalid authorization.</response>
  /// <response code="500">Internal server error.</response> 
  [HttpGet("validatelinkid")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ValidateLinkId(string linkId)
  {
    try
    {
      if (!RegexUtilities.IsValidLinkId(linkId))
      {
        string message = $"{nameof(linkId)} {linkId} is not a valid link ID.";
        LogInformation(message);
        return Problem(detail: message, statusCode: StatusCodes.Status400BadRequest);
      }

      bool linkIdIsValid = await Service.ElectiveCareReferralHasTextMessageWithLinkId(linkId);

      if (linkIdIsValid)
      {
        return Ok();
      }
      else
      {
        return NoContent();
      }
    }
    catch (Exception ex)
    {
      LogException(ex);
      return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
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
