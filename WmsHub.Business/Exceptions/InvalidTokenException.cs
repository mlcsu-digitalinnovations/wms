using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class InvalidTokenException : Exception
{
  public InvalidTokenException() : base() { }
  public InvalidTokenException(string message) : base(message) { }
  public InvalidTokenException(string message, Exception inner)
    : base(message, inner)
  { }
}