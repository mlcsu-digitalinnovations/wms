using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class TriageNotFoundException : Exception
  {
    public TriageNotFoundException() : base() { }
    public TriageNotFoundException(string message) : base(message) { }
    public TriageNotFoundException(string message, Exception inner)
      : base(message, inner)
    { }

    protected TriageNotFoundException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}