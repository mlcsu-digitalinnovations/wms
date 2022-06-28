using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ReferralCreateException : Exception
  {
    public ReferralCreateException() : base() { }
    public ReferralCreateException(string message) : base(message) { }
    public ReferralCreateException(string message, Exception inner)
      : base(message, inner)
    { }

    protected ReferralCreateException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}