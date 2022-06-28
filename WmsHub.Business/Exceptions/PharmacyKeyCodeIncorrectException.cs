using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class PharmacyKeyCodeIncorrectException : Exception
  {
    const string message = "The Security Code you have entered is incorrect.";

    public PharmacyKeyCodeIncorrectException() : base(message) { }
    public PharmacyKeyCodeIncorrectException(string message) : base(message) { }
    public PharmacyKeyCodeIncorrectException(string message, Exception inner)
      : base(message, inner)
    { }

    protected PharmacyKeyCodeIncorrectException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
