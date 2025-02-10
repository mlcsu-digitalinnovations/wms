using System;

namespace WmsHub.Business.Exceptions;

public class StatusChangeException : Exception
{
  public StatusChangeException() : base() { }
  public StatusChangeException(string message) : base(message) { }
  public StatusChangeException(string message, Exception inner)
    : base(message, inner)
  { }
}
