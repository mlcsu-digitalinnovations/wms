using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class NhsNumberTraceMismatchException : Exception
  {
    public NhsNumberTraceMismatchException() : base() { }
    public NhsNumberTraceMismatchException(
      string currentNhsNumber, string tracedNhsNumber)
      : base($"The current NHS number {currentNhsNumber} does not match " +
          $"the traced NHS number {tracedNhsNumber}.") { }
    public NhsNumberTraceMismatchException(string message) : base(message) { }
    public NhsNumberTraceMismatchException(string message, Exception inner)
      : base(message, inner)
    { }

    protected NhsNumberTraceMismatchException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
