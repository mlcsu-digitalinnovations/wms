using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  public class StatusChangeException : Exception
  {
    public StatusChangeException() : base() { }
    public StatusChangeException(string message) : base(message) { }
    public StatusChangeException(string message, Exception inner)
      : base(message, inner)
    { }

    protected StatusChangeException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
