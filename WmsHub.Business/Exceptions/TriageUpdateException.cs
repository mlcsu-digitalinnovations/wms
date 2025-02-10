using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class TriageUpdateException : Exception
{
  public TriageUpdateException() : base() { }
  public TriageUpdateException(string message) : base(message) { }
  public TriageUpdateException(string message, Exception inner)
    : base(message, inner)
  { }
}