using System;

namespace WmsHub.Common.Apis.Ods
{  
  public class OdsOrganisationOptions
  {
    public const string SectionKey = "OdsOrganisationOptions";

    private string endpoint =
      "https://directory.spineservices.nhs.uk/ORD/2-0-0/organisations/";

    public string Endpoint
    {
      get => endpoint;
      set
      {
        endpoint = $"{value.TrimEnd('/')}/"; 
      }
    }
  }
}