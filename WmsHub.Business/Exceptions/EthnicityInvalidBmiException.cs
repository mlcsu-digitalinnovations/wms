using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class EthnicityInvalidBmiException : Exception
{
  public EthnicityInvalidBmiException() : base() { }
  public EthnicityInvalidBmiException(string message) : base(message) { }
  public EthnicityInvalidBmiException(Guid ethnicityId)
    : base($"The BMI Setting of the ethnicity for " +
           $"Id {ethnicityId} is invalid.")
  { }
  public EthnicityInvalidBmiException(string message, Exception inner)
    : base(message, inner)
  { }
}