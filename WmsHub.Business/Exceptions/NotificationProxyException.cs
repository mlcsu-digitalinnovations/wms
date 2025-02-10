using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class NotificationProxyException : Exception
{
  public NotificationProxyException() : base() { }
  public NotificationProxyException(string message) : base(message) { }
  public NotificationProxyException(string message, Exception inner)
    : base(message, inner)
  { }
}