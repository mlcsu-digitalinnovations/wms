using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class DateRangeNotValidException : Exception
{
  public DateRangeNotValidException() : base() { }
  public DateRangeNotValidException(string message) : base(message) { }
  public DateRangeNotValidException(string message, Exception inner)
    : base(message, inner)
  { }
}