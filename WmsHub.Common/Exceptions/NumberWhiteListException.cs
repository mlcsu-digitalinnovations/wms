using System;

namespace WmsHub.Common.Exceptions;

[Serializable]
public class NumberWhiteListException : ArgumentException
{

  public NumberWhiteListException() : base() { }

  public NumberWhiteListException(string message) : base(message)
  { }

  public NumberWhiteListException(string message, Exception innerException)
    : base(message, innerException)
  { }
}