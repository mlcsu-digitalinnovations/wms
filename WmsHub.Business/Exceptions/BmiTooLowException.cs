using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class BmiTooLowException : Exception
{
  public BmiTooLowException() : base() { }
  public BmiTooLowException(string message) : base(message) { }
  public BmiTooLowException(string message, Exception inner)
    : base(message, inner)
  { }
}