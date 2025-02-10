using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class TextMessageExpiredByEmailException : Exception
{
  public TextMessageExpiredByEmailException() : base(
    "User do not want to provide their email address.")
  { }
  public TextMessageExpiredByEmailException(string message)
      : base(message) { }
  public TextMessageExpiredByEmailException(
      string message, Exception inner)
    : base(message, inner)
  { }
}
