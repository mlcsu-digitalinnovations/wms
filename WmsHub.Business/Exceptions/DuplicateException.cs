using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class DuplicateException : Exception
{
  public DuplicateException() : base() { }
  public DuplicateException(string message) : base(message) { }
  public DuplicateException(string message, Exception inner)
    : base(message, inner)
  { }
}