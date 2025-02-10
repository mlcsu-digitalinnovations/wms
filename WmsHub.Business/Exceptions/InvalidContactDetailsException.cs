using System;

namespace WmsHub.Business.Exceptions;
public class InvalidContactDetailsException : Exception
{
  public InvalidContactDetailsException()
  {
  }

  public InvalidContactDetailsException(string message) : base(message)
  {
  }

  public InvalidContactDetailsException(string message, Exception innerException)
    : base(message, innerException)
  {
  }
}
