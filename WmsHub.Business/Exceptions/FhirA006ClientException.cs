using System;

namespace WmsHub.Business.Exceptions;

public class FhirA006ClientException : Exception
{
  public FhirA006ClientException() : base() { }
  public FhirA006ClientException(string message) : base(message) { }
  public FhirA006ClientException(string message, Exception inner)
    : base(message, inner)
  { }
}