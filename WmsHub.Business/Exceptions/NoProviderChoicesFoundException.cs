using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class NoProviderChoicesFoundException : Exception
{
  public NoProviderChoicesFoundException() : base() { }
  public NoProviderChoicesFoundException(Guid? referralId)
    : base($"Unable to find provider choices for the referral {referralId}.")
  { }
  public NoProviderChoicesFoundException(string message) : base(message) { }
  public NoProviderChoicesFoundException(string message, Exception inner)
    : base(message, inner)
  { }
}