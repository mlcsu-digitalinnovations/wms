using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class PharmacyKeyCodeExpiredException : Exception
{
  private const string message = "The Security Code you have entered " +
    "has expired, please request a new Security Code by clicking on the " +
    "email not received link.";

  public PharmacyKeyCodeExpiredException() : base(message) { }
  public PharmacyKeyCodeExpiredException(string message) : base(message) { }
  public PharmacyKeyCodeExpiredException(string message, Exception inner)
    : base(message, inner)
  { }
}
