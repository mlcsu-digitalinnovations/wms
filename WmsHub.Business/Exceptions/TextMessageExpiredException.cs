using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class TextMessageExpiredException : Exception
  {
    public TextMessageExpiredException() : base() { }
    public TextMessageExpiredException(string message) : base(message) { }
    public TextMessageExpiredException(string message, Exception inner) 
      : base(message, inner) 
    { }

    protected TextMessageExpiredException(
      SerializationInfo info,
      StreamingContext context) 
      : base(info, context) 
    { }
  }
}
