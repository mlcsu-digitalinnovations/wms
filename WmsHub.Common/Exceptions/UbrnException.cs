using System;

namespace WmsHub.Common.Exceptions;

[Serializable]
public class UbrnException : ArgumentException
{
  public UbrnException() : base() { }

  public UbrnException(string message) : base(message)
  { }

  public UbrnException(string message, Exception innerException)
    : base(message, innerException)
  { }
}
