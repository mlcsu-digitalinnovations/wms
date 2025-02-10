using System;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.ReferralService.AccessKeys;

public class CreateAccessKeyResponse : ResponseBase, ICreateAccessKeyResponse
{
  public CreateAccessKeyResponse(ErrorTypes errorType, string errorMessage)
  {
    AddErrorMessage(errorType, errorMessage);
  }

  public CreateAccessKeyResponse(
    DateTimeOffset expires,
    string accessKey,
    string email)
  {
    AccessKey = accessKey ?? 
      throw new ArgumentNullException(nameof(accessKey));
    Email = email ?? throw new ArgumentNullException(nameof(email));

    if (expires < DateTimeOffset.Now)
    {
      throw new ArgumentOutOfRangeException(
        nameof(expires),
        $"{nameof(expires)} must be in the future");
    }
    Expires = expires;
  }

  public DateTimeOffset Expires { get; }

  public string AccessKey { get; }

  public string Email { get; }
}