using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ReferralNotUniqueException : Exception
  {
    public ReferralNotUniqueException() : base() { }
    public ReferralNotUniqueException(string message) : base(message) { }
    public ReferralNotUniqueException(string message, Exception inner)
      : base(message, inner)
    { }

    protected ReferralNotUniqueException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
