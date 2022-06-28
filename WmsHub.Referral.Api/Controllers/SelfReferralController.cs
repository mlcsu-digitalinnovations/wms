using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Extensions;
using WmsHub.Referral.Api.Models;

namespace WmsHub.Referral.Api.Controllers
{
  [ApiController]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[Controller]")]
  public class SelfReferralController : BaseController
  {
    private readonly IMapper _mapper;

    public SelfReferralController(IReferralService referralService,
      IMapper mapper)
      : base(referralService)
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
        if (User != null &&
          User.FindFirst(ClaimTypes.Name).Value != "SelfReferral.Service")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access has not been granted for this endpoint.");
        }

        var result = await Service.IsEmailInUseAsync(emailInUse.Email);

        if (result.WasNotFound || result.IsCompleteAndProviderNotSelected)
        {
          return Ok();
        }        
        else
        {
          Log.Debug("Email {email} is already in use by UBRN {ubrn}.",
            result.Referral.Email,
            result.Referral.Ubrn);

          return Problem($"Email {result.Referral.Email} is already in use " +
            $"by UBRN {result.Referral.Ubrn}.",
            statusCode: StatusCodes.Status409Conflict);
        }
      }
      catch (Exception ex)
      {
        if (ex is SelfReferralValidationException)
        {
          LogInformation(ex.Message);
          return ValidationProblem(new ValidationProblemDetails(
            (ex as SelfReferralValidationException).ValidationResults));
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
    /// Creates a self-referral
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