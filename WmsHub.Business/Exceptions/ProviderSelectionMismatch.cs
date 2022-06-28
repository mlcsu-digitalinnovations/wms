using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ProviderSelectionMismatch : Exception
  {
    public ProviderSelectionMismatch() : base() { }
    public ProviderSelectionMismatch(Guid? providerId)
      : base($"Provider {providerId} was not found in the list of providers " +
             $"for the selected Triage Level.")
    { }
    public ProviderSelectionMismatch(string message) : base(message) { }
    public ProviderSelectionMismatch(string message, Exception inner)
      : base(message, inner)
    { }

    protected ProviderSelectionMismatch(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}