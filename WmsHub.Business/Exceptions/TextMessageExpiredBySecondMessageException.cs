using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
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

    protected TextMessageExpiredBySecondMessageException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
