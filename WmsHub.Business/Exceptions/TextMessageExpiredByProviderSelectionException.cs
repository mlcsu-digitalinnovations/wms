using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class TextMessageExpiredByProviderSelectionException : Exception
{
  public TextMessageExpiredByProviderSelectionException() : base() { }
  public TextMessageExpiredByProviderSelectionException(string message)
      : base(message) { }
  public TextMessageExpiredByProviderSelectionException(
      string message, Exception inner)
    : base(message, inner)
  { }
}
