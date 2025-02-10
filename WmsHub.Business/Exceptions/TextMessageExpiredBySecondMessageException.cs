using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class TextMessageExpiredBySecondMessageException : Exception
{
  public TextMessageExpiredBySecondMessageException() : base() { }
  public TextMessageExpiredBySecondMessageException(string message)
      : base(message) { }
  public TextMessageExpiredBySecondMessageException(
      string message, Exception inner)
    : base(message, inner)
  { }
}
