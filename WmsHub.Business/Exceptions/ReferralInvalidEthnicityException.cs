using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class ReferralInvalidEthnicityException : Exception
{
  public ReferralInvalidEthnicityException() : base() { }
  public ReferralInvalidEthnicityException(string message) : base(message) { }
  public ReferralInvalidEthnicityException(string message, Exception inner)
    : base(message, inner)
  { }
}
