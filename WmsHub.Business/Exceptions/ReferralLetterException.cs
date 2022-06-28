using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class ReferralLetterException : Exception
  {
    public ReferralLetterException() : base() { }
    public ReferralLetterException(string message) : base(message) { }
    public ReferralLetterException(string message, Exception inner)
      : base(message, inner)
    { }

    protected ReferralLetterException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
