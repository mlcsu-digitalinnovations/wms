using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class PostcodeNotFoundException : Exception
{
  public PostcodeNotFoundException() : base() { }
  public PostcodeNotFoundException(string message) : base(message) { }
  public PostcodeNotFoundException(string message, Exception inner)
    : base(message, inner)
  { }
}