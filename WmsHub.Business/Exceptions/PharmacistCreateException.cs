using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  public class PharmacistCreateException: Exception
  {
    public PharmacistCreateException() : base() { }
    public PharmacistCreateException(string message) : base(message) { }
    public PharmacistCreateException(string message, Exception inner)
      : base(message, inner)
    { }

    protected PharmacistCreateException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}