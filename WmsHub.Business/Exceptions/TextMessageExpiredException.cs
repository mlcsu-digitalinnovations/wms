using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class TextMessageExpiredException : Exception
{
  public TextMessageExpiredException() : base() { }
  public TextMessageExpiredException(string message) : base(message) { }
  public TextMessageExpiredException(string message, Exception inner)
    : base(message, inner)
  { }
}
