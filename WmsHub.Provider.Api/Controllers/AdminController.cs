using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ProviderRejection;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;

namespace WmsHub.Provider.Api.Controllers
{
  [ApiController]
  [ApiExplorerSettings(IgnoreApi = true)]
  [Authorize(AuthenticationSchemes = "ApiKey")]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[controller]")]
  public class AdminController : BaseController
  {

    public AdminController(IProviderService providerService)
      : base(providerService)
    {
    }

    /// <summary>
    /// Get all Active Provider Details
    /// </summary>
    /// <returns>IActionResult</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get()
    {
      try
      {

        if (User.FindFirst(ClaimTypes.Name).Value != "Provider_admin")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access by admin only");
        }

        ProviderAdminResponse response =
          await Service.GetAllActiveProvidersAsync();

        return BaseReturnResponse(response.ResponseStatus,
          response,
          response.GetErrorMessage());
      }
      catch (Exception ex)
      {
        return BaseBadRequestObjectResult(ex.Message);
      }
    }

    /// <summary>
    /// Gets provider new ApiKey
    /// </summary>
    /// <returns>IActionResult</returns>
    [HttpGet("KeyUpdate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetKeyUpdate(
      Guid providerId, int validDays = 365)
    {
      try
      {

        if (User.FindFirst(ClaimTypes.Name).Value != "Provider_admin")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access by admin only");
        }

        ProviderResponse providerResponse =
          await Service.GetProviderAsync(providerId);

        if (providerResponse.ResponseStatus != StatusType.Valid)
        {
          return BaseReturnResponse(providerResponse.ResponseStatus,
            null,
            providerResponse.GetErrorMessage());
        }

        NewProviderApiKeyResponse response =
            await Service.UpdateProviderKeyAsync(providerResponse, validDays);


        return BaseReturnResponse(response.ResponseStatus,
          response,
          response.GetErrorMessage());
      }
      catch (Exception ex)
      {
        return BaseBadRequestObjectResult(ex.Message);
      }
    }

    /// <summary>
    /// Used to update the provider details shown to the service user as 
    /// they are making their choice of provider.
    /// </summary>
    /// <param name="request">ProviderRequest</param>
    /// <returns></returns>
    [HttpPut("Provider")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(ProviderRequest request)
    {
      try
      {

        if (User.FindFirst(ClaimTypes.Name).Value != "Provider_admin")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access by admin only");
        }

        ProviderResponse providerResponse =
          await Service.UpdateProvidersAsync(request);

        return BaseReturnResponse(providerResponse.ResponseStatus,
          providerResponse,
          providerResponse.GetErrorMessage());
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: (int)HttpStatusCode.InternalServerError);
      }
    }


    /// <summary>
    /// Used to update the provider’s level status to indicate whether the 
    /// provider is accepting service users on each of its 3 level offerings.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("Provider/Status")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutStatus(
      ProviderLevelStatusChangeRequest request)
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "Provider_admin")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access by admin only");
        }

        ProviderResponse response =
          await Service.UpdateProviderLevelsAsync(request);

        return BaseReturnResponse(response.ResponseStatus,
          response,
          response.GetErrorMessage());
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(statusCode: (int)HttpStatusCode.InternalServerError);
      }
    }


    /// <summary>
    /// Update the Providers contact information to setup and maintain the
    /// providers AccessToken and ApiKey
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("ProviderAuth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProviderAuth(
      ProviderAuthUpdateRequest request)
    {
      if (User.FindFirst(ClaimTypes.Name).Value != "Provider_admin")
      {
        return BaseReturnResponse(StatusType.NotAuthorised,
          null,
          "Access by admin only");
      }

      bool isUpdated = await Service.UpdateProviderAuthAsync(request);
      return isUpdated
       ? Ok()
       : StatusCode((int)HttpStatusCode.InternalServerError);
    }

    [HttpGet, Route("RejectionReason")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GetReferralRejectionReasons()
    {
      ProviderRejectionReason[] models = await Service
        .GetRejectionReasonsAsync();

      return Ok(models);
    }

    [HttpPut, Route("RejectionReason")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutReferralRejectionReason(
      [FromBody] ProviderRejectionReasonUpdate request)
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "Provider_admin")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access by admin only");
        }

        ProviderRejectionReasonResponse response =
          await Service.UpdateRejectionReasonsAsync(request);
        return BaseReturnResponse(response.ResponseStatus,
          response, response.GetErrorMessage());
      }
      catch (ProviderRejectionReasonDoesNotExistException ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: (int)HttpStatusCode.NotFound);
      }
      catch (ProviderRejectionReasonMismatchException ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: (int)HttpStatusCode.Conflict);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: (int)HttpStatusCode.InternalServerError);
      }
    }

    [HttpPost, Route("RejectionReason")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostReferralRejectionReason(
      [FromBody] ProviderRejectionReasonSubmission request)
    {
      try
      {
        if (User.FindFirst(ClaimTypes.Name).Value != "Provider_admin")
        {
          return BaseReturnResponse(StatusType.NotAuthorised,
            null,
            "Access by admin only");
        }

        ProviderRejectionReasonResponse response =
          await Service.SetNewRejectionReasonsAsync(request);
        return BaseReturnResponse(response.ResponseStatus,
          response, response.GetErrorMessage());

      }
      catch (ProviderRejectionReasonAlreadyExistsException ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: (int)HttpStatusCode.Conflict);
      }
      catch (Exception ex)
      {
        LogException(ex);
        return Problem(
          detail: ex.Message,
          statusCode: (int)HttpStatusCode.InternalServerError);
      }
    }


    protected internal virtual ProviderService Service
    {
      get
      {
        var service = _service as ProviderService;
        service.User = User;
        return service;
      }
    }
  }
}
