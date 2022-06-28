using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ReferralProviderSelectedException : Exception
  {
    public ReferralProviderSelectedException() : base() { }
    public ReferralProviderSelectedException(
      Guid? referralId, Guid? providerId)
      : base($"The referral {referralId} has previously had its provider " +
          $"selected {providerId}.") 
    { }
    public ReferralProviderSelectedException(string message) : base(message) { }
    public ReferralProviderSelectedException(string message, Exception inner) 
      : base(message, inner) 
    { }

    protected ReferralProviderSelectedException(
      SerializationInfo info,
      StreamingContext context) 
      : base(info, context) 
    { }
  }
}
