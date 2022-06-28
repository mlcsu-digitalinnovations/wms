using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class TriageUpdateException : Exception
  {
    public TriageUpdateException() : base() { }
    public TriageUpdateException(string message) : base(message) { }
    public TriageUpdateException(string message, Exception inner)
      : base(message, inner)
    { }

    protected TriageUpdateException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}