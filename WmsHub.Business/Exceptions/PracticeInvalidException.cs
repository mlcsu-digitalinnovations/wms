using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class PracticeInvalidException : Exception
{
  public PracticeInvalidException() : base() { }
  public PracticeInvalidException(string message) : base(message) { }
  public PracticeInvalidException(string message, Exception inner)
    : base(message, inner)
  { }
}
