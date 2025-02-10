using System;

namespace WmsHub.Common.Exceptions;

[Serializable]
public class EmailNotProvidedException : ArgumentException
{

  public EmailNotProvidedException() : base() { }

  public EmailNotProvidedException(string message) : base(message)
  { }

  public EmailNotProvidedException(string message, Exception innerException)
    : base(message, innerException)
  { }
}