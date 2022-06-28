using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ProviderRejectionReasonDoesNotExistException : Exception
  {
    public ProviderRejectionReasonDoesNotExistException() : base() { }
    public ProviderRejectionReasonDoesNotExistException(string message) 
      : base(message) { }
    public ProviderRejectionReasonDoesNotExistException(
      string message, Exception inner)
      : base(message, inner)
    { }

    protected ProviderRejectionReasonDoesNotExistException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}