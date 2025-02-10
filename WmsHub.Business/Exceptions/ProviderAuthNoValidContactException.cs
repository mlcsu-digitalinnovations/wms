using System;

namespace WmsHub.Business.Exceptions;
public class ProviderAuthNoValidContactException : Exception
{
  public ProviderAuthNoValidContactException()
  {
  }

  public ProviderAuthNoValidContactException(Guid providerId)
    : base($"Provider {providerId} has no valid contact details.")
  {
  }

  public ProviderAuthNoValidContactException(string message) : base(message)
  {
  }

  public ProviderAuthNoValidContactException(string message, Exception innerException)
    : base(message, innerException)
  {
  }
}
