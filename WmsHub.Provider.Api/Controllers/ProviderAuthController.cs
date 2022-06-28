using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Extensions;

namespace WmsHub.Provider.Api.Controllers
{
  [ApiController]
  [Authorize(AuthenticationSchemes = "ApiKey")]
  [ApiVersion("1.0")]
  [ApiVersion("2.0")]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[controller]")]
  public class ProviderAuthController : BaseController
  {

    public ProviderAuthController(IWmsAuthService service) : base(service)
    {
    }

    /// <summary>
    /// Is used to send an Authentication Key to the provider
    /// using either their registered Mobile number or Email address
    /// </summary>
    /// <returns>Ok():BadRequest()</returns>
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
        bool success = await Service.SendNewKeyAsync();
        return success
         ? Ok(
           "A new authentication key has been sent to your registered contact.")
         : StatusCode((int)HttpStatusCode.InternalServerError,
           "There was a problem sending a new key.");
      }
      catch (ProviderAuthCredentialsNotFoundException)
      {
        return Problem(
          detail: "Provider authentication credentials are " +
            "not currently available.",
          statusCode: StatusCodes.Status400BadRequest);
      }
    }

    /// <summary>
    /// Uses the Authentication Key and returns new Access and Refresh Tokens 
    /// </summary>
    /// <param name="key">8 character numeric</param>
    /// <returns>TokenResponse</returns>
    [HttpGet("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string key)
    {
      KeyValidationResponse keyValidationResponse =
        await Service.ValidateKeyAsync(key);

      if (keyValidationResponse.ValidationStatus != ValidationType.ValidKey)
      {

        LogInformation(
          $"Provider with Id[{User.GetUserId()}], had the following " +
          $"{keyValidationResponse.ValidationStatus.ToString()}: " +
          $"{keyValidationResponse.GetErrorMessage()} when using the key" +
          $" {key} ");

        return Problem(
          detail: keyValidationResponse.GetErrorMessage(),
          statusCode: StatusCodes.Status400BadRequest);
      }

      AccessTokenResponse response = await Service.GenerateTokensAsync();
      if (response.ValidationStatus == ValidationType.Valid)
      {
        bool saved = await Service.SaveTokenAsync(response.AccessToken);
        if (!saved)
          return StatusCode((int)HttpStatusCode.InternalServerError);

        TokenResponse token = new TokenResponse()
        {
          RefreshToken = response.RefreshToken,
          Expires = response.Expires,
          AccessToken = response.AccessToken,
          TokenType = response.TokenType
        };
        return Ok(token);
      }

      return Problem(
        detail: response.GetErrorMessage(),
        statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Uses the in date Refresh Token to issue a new Access Token, If the
    /// Refresh Token has been revoked or is out of date, a new Authentication
    /// key is required.
    /// </summary>
    /// <param name="grant_type"></param>
    /// <param name="refresh_token"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post(string grant_type,
      string refresh_token)
    {
      if (string.IsNullOrWhiteSpace(refresh_token))
        return Problem(
          detail: "Refresh token not supplied.",
          statusCode: StatusCodes.Status400BadRequest);

      if (string.IsNullOrWhiteSpace(grant_type))
        return Problem(
          detail: "Grant Type must be supplied.",
          statusCode: StatusCodes.Status400BadRequest);

      if (!grant_type.Equals("refresh_token",
            StringComparison.InvariantCultureIgnoreCase))
        return Problem(
          detail: "Only grant type of refresh_token allowed.",
          statusCode: StatusCodes.Status400BadRequest);

      RefreshTokenValidationResponse validationResponse =
        await Service.ValidateRefreshKeyAsync(refresh_token);

      if (validationResponse.ValidationStatus != ValidationType.Valid)
      {

        LogInformation(
          $"Provider with Id[{User.GetUserId()}], had the following " +
          $"{validationResponse.ValidationStatus.ToString()}: " +
          $"{validationResponse.GetErrorMessage()} when using the refresh_key" +
          $" {refresh_token} ");

        return Problem(
          detail: validationResponse.GetErrorMessage(),
          statusCode: StatusCodes.Status400BadRequest);
      }


      AccessTokenResponse response = await Service.GenerateTokensAsync();
      if (response.ValidationStatus == ValidationType.Valid)
      {
        TokenResponse token = new TokenResponse()
        {
          RefreshToken = response.RefreshToken,
          Expires = response.Expires,
          AccessToken = response.AccessToken,
          TokenType = response.TokenType
        };
        return Ok(token);
      }

      return Problem(
        detail: response.GetErrorMessage(),
        statusCode: StatusCodes.Status400BadRequest);
    }


    private WmsAuthService Service
    {
      get
      {
        WmsAuthService service = _service as WmsAuthService;
        service.User = User;
        return service;
      }
    }
  }
}
