using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ReferralUpdateException : Exception
  {
    public ReferralUpdateException() : base() { }
    public ReferralUpdateException(Guid referralId)
      : base($"Unable to find a referral with an id of {referralId}.") { }
    public ReferralUpdateException(string message) : base(message) { }
    public ReferralUpdateException(string message, Exception inner)
      : base(message, inner)
    { }

    protected ReferralUpdateException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}