using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class PharmacyInvalidException : Exception
  {
    public PharmacyInvalidException() : base() { }
    public PharmacyInvalidException(string message) : base(message) { }
    public PharmacyInvalidException(string message, Exception inner)
      : base(message, inner)
    { }

    protected PharmacyInvalidException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}