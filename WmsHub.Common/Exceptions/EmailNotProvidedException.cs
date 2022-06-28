using System;
using System.Runtime.Serialization;

namespace WmsHub.Common.Exceptions
{
  [Serializable]
  public class EmailNotProvidedException : ArgumentException
  {

    public EmailNotProvidedException() : base() { }

    public EmailNotProvidedException(string message) : base(message)
    { }

    public EmailNotProvidedException(string message, Exception innerException)
      : base(message, innerException)
    { }

    protected EmailNotProvidedException(
      SerializationInfo info, StreamingContext context) : base(info, context)
    { }
  }
}