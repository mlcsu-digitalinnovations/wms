using System;

namespace WmsHub.Common.Exceptions;

[Serializable]
public class EmailWrongDomainException : ArgumentException
{

  public EmailWrongDomainException() : base() { }

  public EmailWrongDomainException(string message) : base(message)
  { }

  public EmailWrongDomainException(string message, Exception innerException)
    : base(message, innerException)
  { }
}