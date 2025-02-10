using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Exceptions;

public class ServiceValidationException : ValidationException
{
  public ServiceValidationException() : base() { }
  public ServiceValidationException(string message) : base(message) { }
  public ServiceValidationException(string message, Exception inner)
    : base(message, inner)
  { }
}
