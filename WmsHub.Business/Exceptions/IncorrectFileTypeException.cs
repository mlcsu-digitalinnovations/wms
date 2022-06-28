using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  public class IncorrectFileTypeException : Exception
  {
    public IncorrectFileTypeException() : base() { }
    public IncorrectFileTypeException(Type type, int value) :
      base($"Invalid enum value {value} for {type.Name}")
    { }
    public IncorrectFileTypeException(string message) : base(message) { }
    public IncorrectFileTypeException(string message, Exception inner)
      : base(message, inner)
    { }

    protected IncorrectFileTypeException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}