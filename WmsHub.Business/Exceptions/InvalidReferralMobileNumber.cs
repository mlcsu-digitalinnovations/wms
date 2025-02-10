using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class InvalidReferralMobileNumber : Exception
{
  public InvalidReferralMobileNumber() : base() { }
  public InvalidReferralMobileNumber(string message) : base(message) { }
  public InvalidReferralMobileNumber(string message, Exception inner)
    : base(message, inner)
  { }
}