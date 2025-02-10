using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class InvalidApiKeyException : Exception
{
  public InvalidApiKeyException() : base() { }
  public InvalidApiKeyException(string message) : base(message) { }
  public InvalidApiKeyException(string message, Exception inner)
    : base(message, inner)
  { }
}