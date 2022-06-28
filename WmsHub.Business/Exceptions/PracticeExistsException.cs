using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class PracticeExistsException : Exception
  {
    public PracticeExistsException() : base() { }
    public PracticeExistsException(string odsCode)
      : base($"Practice with an OdsCode of {odsCode} already exists.") { }
    public PracticeExistsException(string message, Exception inner)
      : base(message, inner)
    { }

    protected PracticeExistsException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
