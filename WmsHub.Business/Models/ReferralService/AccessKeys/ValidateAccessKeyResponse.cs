using System;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.ReferralService.AccessKeys;

public class ValidateAccessKeyResponse
  : ResponseBase, IValidateAccessKeyResponse
{
  public ValidateAccessKeyResponse(string errorMessage, ErrorTypes errorType)
  {
    AddErrorMessage(errorType, errorMessage);
  }

  public ValidateAccessKeyResponse(bool isValidCode, DateTimeOffset expires)
  {
    if (expires < DateTimeOffset.Now)
    {
      throw new ArgumentOutOfRangeException(
        nameof(expires),
        $"{nameof(expires)} must be in the future");
    }
    Expires = expires;

    IsValidCode = isValidCode;
  }

  public bool IsValidCode { get; }

  public DateTimeOffset Expires { get; }
}