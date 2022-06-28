using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class PharmacyExistsException : Exception
  {
    public PharmacyExistsException() : base() { }
    public PharmacyExistsException(string odsCode)
      : base($"Pharmacy with an OdsCode of {odsCode} already exists.") { }
    public PharmacyExistsException(string message, Exception inner)
      : base(message, inner)
    { }

    protected PharmacyExistsException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}