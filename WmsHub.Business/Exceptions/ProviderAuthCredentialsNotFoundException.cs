using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ProviderAuthCredentialsNotFoundException : Exception
  {
    public ProviderAuthCredentialsNotFoundException() : base() { }
    public ProviderAuthCredentialsNotFoundException(Guid providerId)
      : base($"Unable to find a provider auth with an id of {providerId}.")
    { }
    public ProviderAuthCredentialsNotFoundException(string message)
      : base(message)
    { }

    protected ProviderAuthCredentialsNotFoundException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}