using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class EthnicityNotFoundException : Exception
{
  public EthnicityNotFoundException() : base() { }
  public EthnicityNotFoundException(string message) : base(message) { }
  public EthnicityNotFoundException(Guid ethnicityId)
  : base($"Unable to find an ethnicity with an id of {ethnicityId}.") { }
  public EthnicityNotFoundException(string message, Exception inner)
    : base(message, inner)
  { }
}