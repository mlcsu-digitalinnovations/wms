using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class InvalidReferralId : Exception
  {
    public InvalidReferralId() : base() { }
    public InvalidReferralId(string message) : base(message) { }
    public InvalidReferralId(string message, Exception inner)
      : base(message, inner)
    { }

    protected InvalidReferralId(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}