using System;

namespace WmsHub.Business.Exceptions;
public class MskOrganisationNotFoundException : Exception
{
  public MskOrganisationNotFoundException()
  {
  }

  public MskOrganisationNotFoundException(string message) : base(message)
  {
  }

  public MskOrganisationNotFoundException(
    string message,
    Exception innerException) : base(message, innerException)
  {
  }
}
