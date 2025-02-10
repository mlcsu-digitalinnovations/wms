using System;

namespace WmsHub.Business.Exceptions;
public class ReferralSourceNotFoundException : Exception
{
  public ReferralSourceNotFoundException() : base() { }
  public ReferralSourceNotFoundException(string message) : base(message) { }
  public ReferralSourceNotFoundException(string message, Exception inner)
    : base(message, inner)
  { }
}
