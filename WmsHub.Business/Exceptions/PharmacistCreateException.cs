using System;

namespace WmsHub.Business.Exceptions;

public class PharmacistCreateException : Exception
{
  public PharmacistCreateException() : base() { }
  public PharmacistCreateException(string message) : base(message) { }
  public PharmacistCreateException(string message, Exception inner)
    : base(message, inner)
  { }
}