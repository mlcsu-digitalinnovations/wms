using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ReferralInvalidStatusException : Exception
  {
    public ReferralInvalidStatusException() : base() { }
    public ReferralInvalidStatusException(string message) : base(message) { }
    public ReferralInvalidStatusException(string message, Exception inner) 
      : base(message, inner) 
    { }

    protected ReferralInvalidStatusException(
      SerializationInfo info,
      StreamingContext context) 
      : base(info, context) 
    { }
  }
}
