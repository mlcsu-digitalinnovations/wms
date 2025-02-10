using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class StatusOverrideException : Exception
{
  public StatusOverrideException() : base() { }
  public StatusOverrideException(string message) : base(message) { }
  public StatusOverrideException(string message, Exception inner)
    : base(message, inner)
  { }
}
