using System;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class DeprivationNotFoundException : Exception
  {
    public DeprivationNotFoundException() : base() { }
    public DeprivationNotFoundException(string message) : base(message) { }
    public DeprivationNotFoundException(string message, Exception inner)
      : base(message, inner)
    { }

    protected DeprivationNotFoundException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    { }
  }
}