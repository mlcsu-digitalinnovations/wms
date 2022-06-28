using System;
using System.Runtime.Serialization;

namespace WmsHub.Common.Exceptions
{
  [Serializable]
  public class ArgumentNullOrWhiteSpaceException : ArgumentException
  {
    const string MESSAGE = "Value cannot be null or white space.";

    public ArgumentNullOrWhiteSpaceException()
      : base(MESSAGE)
    {
    }

    public ArgumentNullOrWhiteSpaceException(string paramName)
      : base(MESSAGE, paramName)
    {
    }

    public ArgumentNullOrWhiteSpaceException(string message, string paramName)
      : base(message, paramName)
    {
    }

    public ArgumentNullOrWhiteSpaceException(
      string message, Exception innerException)
      : base(message, innerException)
    {
    }

    protected ArgumentNullOrWhiteSpaceException(
      SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}