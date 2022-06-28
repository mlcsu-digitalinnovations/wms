using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
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

    protected TextMessageExpiredByProviderSelectionException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
