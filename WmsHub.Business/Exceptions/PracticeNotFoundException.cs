using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class PracticeNotFoundException : Exception
  {
    public PracticeNotFoundException() : base() { }
    public PracticeNotFoundException(string odsCode)
      : base($"Unable to find a practice with an OdsCode of {odsCode}.") { }
    public PracticeNotFoundException(string message, Exception inner)
      : base(message, inner)
    { }

    protected PracticeNotFoundException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
