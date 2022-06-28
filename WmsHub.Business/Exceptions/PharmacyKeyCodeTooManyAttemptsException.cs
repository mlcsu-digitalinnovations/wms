using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class PharmacyKeyCodeTooManyAttemptsException : Exception
  {
    const string message = "You have exhausted all your allowable attempts " +
      "to access the system, please request a new Security Code by clicking " +
      "on the email not received link.";

    public PharmacyKeyCodeTooManyAttemptsException() : base(message) { }
    public PharmacyKeyCodeTooManyAttemptsException(string message) 
      : base(message) { }
    public PharmacyKeyCodeTooManyAttemptsException(
      string message, Exception inner) : base(message, inner)
    { }

    protected PharmacyKeyCodeTooManyAttemptsException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
