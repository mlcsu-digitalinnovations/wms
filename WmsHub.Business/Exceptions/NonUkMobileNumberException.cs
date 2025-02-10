using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class NonUkMobileNumberException : Exception
{
  public NonUkMobileNumberException()
  { }

  public NonUkMobileNumberException(string message) : base(message)
  { }

  public NonUkMobileNumberException(string message, Exception innerException)
    : base(message, innerException)
  { }
}
