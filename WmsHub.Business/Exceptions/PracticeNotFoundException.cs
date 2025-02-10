using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class PracticeNotFoundException : Exception
{
  public PracticeNotFoundException() : base() { }
  public PracticeNotFoundException(string odsCode)
    : base($"Unable to find a practice with an OdsCode of {odsCode}.") { }
  public PracticeNotFoundException(string message, Exception inner)
    : base(message, inner)
  { }
}
