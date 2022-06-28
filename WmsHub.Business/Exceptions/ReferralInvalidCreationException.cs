using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ReferralInvalidCreationException : Exception
  {
    public ReferralInvalidCreationException() : base() { }
    public ReferralInvalidCreationException(string message) : base(message) { }
    public ReferralInvalidCreationException(string message, Exception inner)
      : base(message, inner)
    { }

    protected ReferralInvalidCreationException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
