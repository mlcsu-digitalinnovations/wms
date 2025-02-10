using System;

namespace WmsHub.Business.Exceptions;
public class ProcessAlreadyRunningException : Exception
{
  public ProcessAlreadyRunningException() : base() { }
  public ProcessAlreadyRunningException(string message) : base(message) { }
  public ProcessAlreadyRunningException(string message, Exception inner)
    : base(message, inner)
  { }
}
