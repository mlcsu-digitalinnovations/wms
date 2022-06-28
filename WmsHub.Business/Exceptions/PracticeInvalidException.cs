using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class PracticeInvalidException : Exception
  {
    public PracticeInvalidException() : base() { }
    public PracticeInvalidException(string message) : base(message) { }
    public PracticeInvalidException(string message, Exception inner)
      : base(message, inner)
    { }

    protected PracticeInvalidException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
