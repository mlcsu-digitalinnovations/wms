using System;

namespace WmsHub.ReferralsService.Exceptions;

public class SmartCardException : Exception
{
  public SmartCardException() : base() { }

  public SmartCardException(string message) : base(message)
  { }

  public SmartCardException(string message, Exception innerException)
    : base(message, innerException)
  { }
}
