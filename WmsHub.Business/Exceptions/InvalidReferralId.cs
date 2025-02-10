using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class InvalidReferralId : Exception
{
  public InvalidReferralId() : base() { }
  public InvalidReferralId(string message) : base(message) { }
  public InvalidReferralId(string message, Exception inner)
    : base(message, inner)
  { }
}