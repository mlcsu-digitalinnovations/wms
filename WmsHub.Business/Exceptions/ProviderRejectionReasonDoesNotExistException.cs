using System;

namespace WmsHub.Business.Exceptions;

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
}