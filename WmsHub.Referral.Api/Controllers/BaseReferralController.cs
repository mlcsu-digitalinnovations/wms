using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Threading.Tasks;
using System;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ReferralService.AccessKeys;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Common.Api.Controllers;
using WmsHub.Common.Helpers;
using WmsHub.Referral.Api.Models;
using ValidateAccessKeyResponse =
  WmsHub.Referral.Api.Models.ValidateAccessKeyResponse;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Referral.Api.Controllers;

public abstract class BaseReferralController<T>
  : BaseController
  where T : EmailDomainWhitelistBase
{
  protected readonly ILogger _log;
  protected readonly T _options;

  public BaseReferralController(
    ILogger logger,
    T options,
    IServiceBase service)
    : base(service)
  {
    _log = logger;
    _options = options;
  }

  protected abstract int MaxActiveAccessKeys { get; }

  protected virtual async Task<IActionResult> GenerateAccessKey(
    AccessKeyType accessKeyType,
    string email,
    int expireMinutes)
  {
    try
    {
      if (!RegexUtilities.IsValidEmail(email))
      {
        _log.Debug(
          "Email {Email} passed to GenerateKey is invalid.",
          email);

        return Problem(
          detail: "The Email field is not a valid email address.",
          statusCode: StatusCodes.Status400BadRequest,
          type: ResponseBase.ErrorTypes.Validation.ToString());
      }

      if (!_options.IsEmailDomainInWhitelist(email))
      {
        _log.Information(
          "Email {Email} passed to GenerateKey has a domain that is not " +
            "in the email domain whitelist {@DomainWhiteList}.",
          email,
          _options.EmailDomainWhitelist);

        return Problem(
          detail: "Email's domain is not in the domain white list.",
          statusCode: StatusCodes.Status403Forbidden,
          type: ResponseBase.ErrorTypes.Whitelist.ToString());
      }

      ICreateAccessKeyResponse response = 
        await Service.CreateAccessKeyAsync(new CreateAccessKey
        {
          Email = email,
          ExpireMinutes = expireMinutes < 0 ? 10 : expireMinutes,
          AccessKeyType = AccessKeyType.StaffReferral,
          MaxActiveAccessKeys = MaxActiveAccessKeys
        });

      if (response.HasErrors)
      {
        switch (response.ErrorType)
        {
          case ResponseBase.ErrorTypes.Validation:
            return Problem(
              detail: response.GetErrorMessage(),
              statusCode: StatusCodes.Status400BadRequest,
              type: response.ErrorType.ToString());

          case ResponseBase.ErrorTypes.MaxActiveAccessKeys:
            return Problem(
              detail: response.GetErrorMessage(),
              statusCode: StatusCodes.Status429TooManyRequests,
              type: response.ErrorType.ToString());

          default:
            string accessType =
              accessKeyType == AccessKeyType.StaffReferral ? "Staff" : "Msk";
            _log.Error($"Create{accessType}AccessKeyAsync responded with " +
              "unknown error type {errorType}. {errorMessage}",
              response.ErrorType,
              response.GetErrorMessage());

            return Problem(
              detail: response.GetErrorMessage(),
              statusCode: StatusCodes.Status500InternalServerError,
              type: response.ErrorType.ToString());
        }
      }

      return base.Ok(new Models.CreateAccessKeyResponse
      {
        Expires = response.Expires,
        KeyCode = response.AccessKey,
      });
    }
    catch (Exception ex)
    {
      _log.Error(ex, "GenerateKey failed unexpectedly.");

      return Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError);
    }
  }

  protected virtual async Task<IActionResult> ValidateAccessKey(
      AccessKeyType accessKeyType,
      string email,
      string keyCode)
  {
    try
    {
      if (!RegexUtilities.IsValidEmail(email))
      {
        _log.Debug(
          "Email {Email} passed to GenerateKey is invalid.",
          email);

        return Problem(
          detail: "The Email field is not a valid email address.",
          statusCode: StatusCodes.Status400BadRequest,
          type: ResponseBase.ErrorTypes.Validation.ToString());
      }

      if (!_options.IsEmailDomainInWhitelist(email))
      {
        _log.Information(
          "Email {Email} passed to ValidateAccessKey has a domain that is " +
          "not in the email domain whitelist {@DomainWhiteList}.",
          email);

        return Problem(
          detail: "Email's domain is not in the domain white list.",
          statusCode: StatusCodes.Status403Forbidden,
          type: ResponseBase.ErrorTypes.Whitelist.ToString());
      }

      IValidateAccessKeyResponse response = await Service
        .ValidateAccessKeyAsync(new ValidateAccessKey()
        {
          AccessKey = keyCode,
          Email = email,
          Type = AccessKeyType.StaffReferral,
          MaxActiveAccessKeys = MaxActiveAccessKeys,
        });

      if (response.HasErrors)
      {
        switch (response.ErrorType)
        {
          case ResponseBase.ErrorTypes.Validation:
            _log.Debug(response.GetErrorMessage());
            return Problem(
              detail: response.GetErrorMessage(),
              statusCode: StatusCodes.Status400BadRequest,
              type: response.ErrorType.ToString());

          case ResponseBase.ErrorTypes.NotFound:
            _log.Debug(response.GetErrorMessage());
            return Problem(
              detail: response.GetErrorMessage(),
              statusCode: StatusCodes.Status404NotFound,
              type: response.ErrorType.ToString());

          case ResponseBase.ErrorTypes.Expired:
            _log.Debug(response.GetErrorMessage());
            return Problem(
              detail: "The Security Code you have entered has expired, " +
                "please request a new Security Code by clicking on the email " +
                "not received link.",
              statusCode: StatusCodes.Status422UnprocessableEntity,
              type: response.ErrorType.ToString());

          case ResponseBase.ErrorTypes.TooManyAttempts:
            _log.Debug(response.GetErrorMessage());
            return Problem(
              detail: "You have exhausted all your allowable attempts to " +
                "access the system, please request a new Security Code by " +
                "clicking on the email not received link.",
              statusCode: StatusCodes.Status422UnprocessableEntity,
              type: response.ErrorType.ToString());

          case ResponseBase.ErrorTypes.Incorrect:
            _log.Debug(response.GetErrorMessage());
            return Problem(
              detail: "The Security Code you have entered is incorrect.",
              statusCode: StatusCodes.Status422UnprocessableEntity,
              type: response.ErrorType.ToString());

          default:
            string accessType =
              accessKeyType == AccessKeyType.StaffReferral ? "Staff" : "Msk";
            _log.Error($"Validate{accessType}AccessKeyAsync " +
              "responded with unknown " +
              "error type {errorType}. {errorMessage}",
              response.ErrorType,
              response.GetErrorMessage());

            return Problem(
              detail: response.GetErrorMessage(),
              statusCode: StatusCodes.Status500InternalServerError,
              type: response.ErrorType.ToString());
        }
      }

      return Ok(new ValidateAccessKeyResponse()
      {
        Expires = response.Expires,
        ValidCode = response.IsValidCode
      });
    }
    catch (Exception ex)
    {
      _log.Error(ex, "ValidateAccessKey failed unexpectedly.");

      return Problem(
        detail: ex.Message,
        statusCode: StatusCodes.Status500InternalServerError,
        type: ResponseBase.ErrorTypes.Unknown.ToString());
    }
  }

  protected IReferralService Service
  {
    get
    {
      IReferralService service = _service as IReferralService;
      service.User = User;
      return service;
    }
  }
}
