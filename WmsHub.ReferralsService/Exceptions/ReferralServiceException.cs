using System;

namespace WmsHub.ReferralsService.Exceptions;

public class ReferralServiceException : Exception
{
  public ReferralServiceException() : base() { }

  public ReferralServiceException(string message) : base(message)
  { }

  public ReferralServiceException(string message, Exception innerException)
    : base(message, innerException)
  { }
}
