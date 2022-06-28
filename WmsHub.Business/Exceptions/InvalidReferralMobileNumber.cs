using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class InvalidReferralMobileNumber : Exception
  {
    public InvalidReferralMobileNumber() : base() { }
    public InvalidReferralMobileNumber(string message) : base(message) { }
    public InvalidReferralMobileNumber(string message, Exception inner)
      : base(message, inner)
    { }

    protected InvalidReferralMobileNumber(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}