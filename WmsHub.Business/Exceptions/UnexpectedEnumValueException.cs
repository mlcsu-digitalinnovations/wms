using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  public class UnexpectedEnumValueException : Exception
  {
    public UnexpectedEnumValueException() : base() { }
		public UnexpectedEnumValueException(Type type, int value) :
			base($"Invalid enum value {value} for {type.Name}") { }
    public UnexpectedEnumValueException(string message) : base(message) { }
    public UnexpectedEnumValueException(string message, Exception inner)
      : base(message, inner)
    { }

    protected UnexpectedEnumValueException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}
