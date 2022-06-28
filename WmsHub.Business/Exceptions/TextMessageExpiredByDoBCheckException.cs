using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class TextMessageExpiredByDoBCheckException : Exception
  {
    public TextMessageExpiredByDoBCheckException() : base() { }
    public TextMessageExpiredByDoBCheckException(string message)
			: base(message) { }
    public TextMessageExpiredByDoBCheckException(
			string message, Exception inner)
      : base(message, inner)
    { }

    protected TextMessageExpiredByDoBCheckException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
