using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class TextMessageExpiredByEmailException : Exception
  {
    public TextMessageExpiredByEmailException() : base(
      "User do not want to provide their email address.") { }
    public TextMessageExpiredByEmailException(string message)
			: base(message) { }
    public TextMessageExpiredByEmailException(
			string message, Exception inner)
      : base(message, inner)
    { }

    protected TextMessageExpiredByEmailException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
