using System;

namespace WmsHub.Business.Exceptions;

public class StatusNotFoudException : Exception
{
  public StatusNotFoudException() : base() { }
  public StatusNotFoudException(string message) : base(message) { }
  public StatusNotFoudException(string message, Exception inner)
    : base(message, inner)
  { }
}
