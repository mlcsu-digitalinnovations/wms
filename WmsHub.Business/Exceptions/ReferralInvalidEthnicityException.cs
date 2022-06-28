using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ReferralInvalidEthnicityException : Exception
  {
    public ReferralInvalidEthnicityException() : base() { }
    public ReferralInvalidEthnicityException(string message) : base(message) { }
    public ReferralInvalidEthnicityException(string message, Exception inner)
      : base(message, inner)
    { }

    protected ReferralInvalidEthnicityException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
