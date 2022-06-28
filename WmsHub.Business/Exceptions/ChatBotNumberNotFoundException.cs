using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ChatBotNumberNotFoundException : Exception
  {
    public ChatBotNumberNotFoundException() : base() { }
    public ChatBotNumberNotFoundException(string message) : base(message) { }
    public ChatBotNumberNotFoundException(string message, Exception inner) 
      : base(message, inner) 
    { }

    protected ChatBotNumberNotFoundException(
      SerializationInfo info,
      StreamingContext context) 
      : base(info, context) 
    { }
  }
}
