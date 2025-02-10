using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class MessageTemplateNotFoundException : Exception
{
  public MessageTemplateNotFoundException() : base() { }
  public MessageTemplateNotFoundException(string message) : base(message) { }
  public MessageTemplateNotFoundException(string message, Exception inner)
    : base(message, inner)
  { }
}
