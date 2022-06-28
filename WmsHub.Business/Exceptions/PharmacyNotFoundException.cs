using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class PharmacyNotFoundException : Exception
  {
    public PharmacyNotFoundException() : base() { }
    public PharmacyNotFoundException(string odsCode)
      : base($"Unable to find a pharmacy with an OdsCode of {odsCode}.") { }
    public PharmacyNotFoundException(string message, Exception inner)
      : base(message, inner)
    { }

    protected PharmacyNotFoundException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}