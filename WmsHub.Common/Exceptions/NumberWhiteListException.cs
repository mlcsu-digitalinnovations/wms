using System;
using System.Runtime.Serialization;

namespace WmsHub.Common.Exceptions
{
  [Serializable]
  public class NumberWhiteListException : ArgumentException
  {

    public NumberWhiteListException() : base() { }

    public NumberWhiteListException(string message) : base(message)
    { }

    public NumberWhiteListException(string message, Exception innerException)
      : base(message, innerException)
    {}

    protected NumberWhiteListException(
      SerializationInfo info, StreamingContext context) : base(info, context)
    {}
  }
}