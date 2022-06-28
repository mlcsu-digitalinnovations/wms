using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class AgeOutOfRangeException : Exception
  {
    public AgeOutOfRangeException() : base() { }
    public AgeOutOfRangeException(string message) : base(message) { }
    public AgeOutOfRangeException(string message, Exception inner)
      : base(message, inner)
    { }

    protected AgeOutOfRangeException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}