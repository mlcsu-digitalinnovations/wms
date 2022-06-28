using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ProviderNotFoundException : Exception
  {
    public ProviderNotFoundException() : base() { }
    public ProviderNotFoundException(Guid? providerId)
      : base($"Unable to find a provider with an id of {providerId}.") 
    { }
    public ProviderNotFoundException(string message) : base(message) { }
    public ProviderNotFoundException(string message, Exception inner) 
      : base(message, inner) 
    { }

    protected ProviderNotFoundException(
      SerializationInfo info,
      StreamingContext context) 
      : base(info, context) 
    { }
  }
}
