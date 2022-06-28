using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Extensions;
using WmsHub.Referral.Api.Models.GeneralReferral;
using static WmsHub.Referral.Api.Models.GeneralReferral.GetNhsNumberConflictResponse;

namespace WmsHub.Referral.Api.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
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
    /// Returns a general referral that matches the provided NHS number if its 
    /// status is such that it can be updated and a provider selected.
    /// </summary>
    /// <remarks>
    /// A 200 response with the associated referral object will be returned if 
    /// the provided NHS number:
    /// <ul><li>Matches a general referral which has not had a provider 
    /// selected.</li></ul>
    /// A 204 response will be returned if the provided NHS number:
    /// <ul><li>Does not match a referral.</li>
    /// <li>Matches a referral which is currently cancelled and a provider was 
    /// not selected.</li>
    /// A 409 response will be returned if the provided NHS number:
    /// <li>Matches a referral which is active but is not a general 
    /// referral.</li>
    /// <li>Matches a general referral that has had a provider selected.
    /// </li></ul>
    /// </remarks>
    /// <param name="nhsNumber">A valid NHS Number.</param>
    /// <response code="200">Request successful, updateable referral returned.
    /// </response>
    /// <response code="204">Request successful and a new referral can be 
    /// created with this NHS number.</response>
    /// <response code="400">NHS Number is invalid.</response>
    /// <response code="401">Invalid authentication</response>
    /// <response code="409">A referral exists that cannot be 
    /// updated and a new referral cannot be created with this NHS number.
    /// </response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{nhsNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK,
      Type = typeof(GetNhsNumberOkResponse))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest,
      Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict,
      Type = typeof(GetNhsNumberConflictResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> GetNhsNumber(string nhsNumber)
    {
      try
      {
        if (User == null ||
          User.FindFirst(ClaimTypes.Name)?.Value != "GeneralReferral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        GetNhsNumberRequest request = new GetNhsNumberRequest
        {
          NhsNumber = nhsNumber
        };

        request.InjectionRemover();

        ValidationModel nhsNumberValidationResult = request.IsValid();

        if (!nhsNumberValidationResult.IsValid)
        {
          LogInformation(nhsNumberValidationResult.Error);
          return Problem(nhsNumberValidationResult.Error,
            statusCode: StatusCodes.Status400BadRequest);
        }

        InUseResponse result =
          await Service.IsNhsNumberInUseAsync(request.NhsNumber);

        // Existing referral?
        if (result.InUseResult.HasFlag(InUseResult.NotFound))
        {
          // No
          return NoContent();
        }
        // Yes, Is referral cancelled?
        else if (result.InUseResult.HasFlag(InUseResult.Cancelled))
        {
          // Yes, Was provider selected?
          if (result.InUseResult.HasFlag(InUseResult.ProviderSelected))
          {
            // Yes
            return Conflict(new GetNhsNumberConflictResponse(
              ErrorType.PreviousReferralCancelled, result));
          }
          else if (result.InUseResult
            .HasFlag(InUseResult.ProviderNotSelected))
          {
            // No
            return NoContent();
          }
          else
          {
            string message = $"Invalid {nameof(result.InUseResult)} flag " +
              $"'{result.InUseResult}'.";
            Log.Error(message);
            return Problem(message,
              statusCode: StatusCodes.Status500InternalServerError);
          }
        }
        // Is referral complete?
        else if (result.InUseResult.HasFlag(InUseResult.Complete))
        {
          // Yes
          return Conflict(new GetNhsNumberConflictResponse(
            ErrorType.PreviousReferralCompleted, result));
        }
        // No, Is referral source general?
        else if (result.InUseResult.HasFlag(InUseResult.IsGeneralReferral))
        {
          // Yes, Is provider selected?
          if (result.InUseResult.HasFlag(InUseResult.ProviderSelected))
          {
            // Yes
            return Conflict(new GetNhsNumberConflictResponse(
              ErrorType.ProviderPreviouslySelected, result));
          }
          else if (result.InUseResult
            .HasFlag(InUseResult.ProviderNotSelected))
          {
            // No
            GetNhsNumberOkResponse getNhsNumberOkResponse = _mapper
              .Map<GetNhsNumberOkResponse>(result.Referral);

            return Ok(getNhsNumberOkResponse);
          }
          else
          {
            string message = $"Invalid {nameof(result.InUseResult)} flag " +
              $"'{result.InUseResult}'.";
            Log.Error(message);
            return Problem(message,
              statusCode: StatusCodes.Status500InternalServerError);
          }
        }
        else if (result.InUseResult
          .HasFlag(InUseResult.IsNotGeneralReferral))
        {
          // No
          return Conflict(new GetNhsNumberConflictResponse(
            ErrorType.OtherReferralSource, result));
        }
        else
        {
          string message = $"Invalid {nameof(result.InUseResult)} flag " +
            $"'{result.InUseResult}'.";
          Log.Error(message);
          return Problem(message,
            statusCode: StatusCodes.Status500InternalServerError);
        }
      }
      catch (Exception ex)
      {
        LogInformation(ex.Message);
        return Problem(ex.Message,
          statusCode: StatusCodes.Status500InternalServerError);
      }
    }

    /// <summary>
    /// Creates a general referral.
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
        if (User.FindFirst(ClaimTypes.Name).Value != "GeneralReferral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

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
        if (User?.FindFirst(ClaimTypes.Name)?.Value != "GeneralReferral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

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
        if (User.FindFirst(ClaimTypes.Name).Value != "GeneralReferral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        request.InjectionRemover();

        ValidationModel validationResult = request.IsValid();
        if (!validationResult.IsValid)
        {
          LogInformation(validationResult.Error);
          return Problem(validationResult.Error,
            statusCode: StatusCodes.Status400BadRequest);
        }

        await Service.UpdateReferralWithProviderAsync(request.Id,
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
