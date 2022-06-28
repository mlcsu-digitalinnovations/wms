using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ReferralNotFoundException : Exception
  {
    public ReferralNotFoundException() : base() { }
    public ReferralNotFoundException(Guid referralId)
      : base($"Unable to find a referral with an id of {referralId}.") { }
    public ReferralNotFoundException(Guid? referralId)
      : base($"Unable to find a referral with an id of {referralId ?? null}.") { }
    public ReferralNotFoundException(string message) : base(message) { }
    public ReferralNotFoundException(string message, Exception inner)
      : base(message, inner)
    { }

    protected ReferralNotFoundException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
