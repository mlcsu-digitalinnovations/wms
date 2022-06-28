using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ReferralContactEmailException : Exception
  {
    public ReferralContactEmailException() : base() { }
    public ReferralContactEmailException(string message) : base(message) { }
    public ReferralContactEmailException(string message, Exception inner)
      : base(message, inner)
    { }

    protected ReferralContactEmailException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
