using System;

namespace WmsHub.Common.Exceptions;

[Serializable]
public class EmailWhiteListException : ArgumentException
{

  public EmailWhiteListException() : base() { }

  public EmailWhiteListException(string message) : base(message)
  { }

  public EmailWhiteListException(string message, Exception innerException)
    : base(message, innerException)
  { }
}