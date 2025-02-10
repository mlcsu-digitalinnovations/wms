using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class DelayReferralException : Exception
{
  public DelayReferralException() : base() { }
  public DelayReferralException(string message) : base(message) { }
  public DelayReferralException(string message, Exception inner)
    : base(message, inner)
  { }
}