using System;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class OrganisationNotFoundException : Exception
{
  public OrganisationNotFoundException() : base() { }
  public OrganisationNotFoundException(string odsCode)
    : base($"Unable to find an organisation with an OdsCode of {odsCode}.")
  { }

  public OrganisationNotFoundException(string message, Exception inner)
    : base(message, inner)
  { }
}
