using System;

namespace WmsHub.Business.Exceptions;

public class TemplateNotFoundException : Exception
{
  public TemplateNotFoundException() : base() { }
  public TemplateNotFoundException(string message) : base(message) { }
  public TemplateNotFoundException(string message, Exception inner)
    : base(message, inner)
  { }

  public TemplateNotFoundException(
    string type,
    string status,
    string source)
    : base($"{type} Template not found for ReferralStatus {status} " +
      $"and ReferralSource {source}.")
  { }
}
