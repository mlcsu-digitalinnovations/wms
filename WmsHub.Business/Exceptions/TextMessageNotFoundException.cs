using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class TextMessageNotFoundException : Exception
{
  public TextMessageNotFoundException() : base() { }
  public TextMessageNotFoundException(string message) : base(message) { }
  public TextMessageNotFoundException(string message, Exception inner)
    : base(message, inner)
  { }
}
