using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class TriageNotFoundException : Exception
{
  public TriageNotFoundException() : base() { }
  public TriageNotFoundException(string message) : base(message) { }
  public TriageNotFoundException(string message, Exception inner)
    : base(message, inner)
  { }
}