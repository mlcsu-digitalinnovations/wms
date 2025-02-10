using System;

namespace WmsHub.Business.Exceptions;
public class ClaimsPrincipalNullException : Exception
{
  public ClaimsPrincipalNullException()
  {
  }

  public ClaimsPrincipalNullException(string claimsPrincipalName)
    : base($"ClaimsPrincipal {claimsPrincipalName} is null.")
  {
  }

  public ClaimsPrincipalNullException(string message, Exception innerException)
    : base(message, innerException)
  {
  }
}
