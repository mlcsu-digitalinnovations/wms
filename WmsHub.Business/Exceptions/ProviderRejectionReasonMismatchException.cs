using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ProviderRejectionReasonMismatchException : Exception
  {
    public ProviderRejectionReasonMismatchException() : base() { }
    public ProviderRejectionReasonMismatchException(string message) : base(message) { }
    public ProviderRejectionReasonMismatchException(string message, Exception inner)
      : base(message, inner)
    { }

    protected ProviderRejectionReasonMismatchException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}