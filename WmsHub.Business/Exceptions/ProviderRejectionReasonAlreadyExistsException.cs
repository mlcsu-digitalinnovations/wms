using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class ProviderRejectionReasonAlreadyExistsException : Exception
{
  public ProviderRejectionReasonAlreadyExistsException() : base() { }
  public ProviderRejectionReasonAlreadyExistsException(string message) : base(message) { }
  public ProviderRejectionReasonAlreadyExistsException(string message, Exception inner)
    : base(message, inner)
  { }
}