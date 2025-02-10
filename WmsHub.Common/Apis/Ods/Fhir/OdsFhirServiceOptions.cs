namespace WmsHub.Common.Apis.Ods.Fhir;

public class OdsFhirServiceOptions
{
  public const string SectionKey = "OdsFhirServiceOptions";
  internal const string BASE_URL = "https://directory.spineservices.nhs.uk/";

  private const string ORGANISATION_PATH = "STU3/Organization/";  

  public string BaseUrl
  {
    get => _baseUrl;
    set => _baseUrl = value.EndsWith("/") ? value : value + "/";
  }
  public string OrganisationPath
  {
    get => _organisationPath;
    set => _organisationPath = value.EndsWith("/") ? value : value + "/";
  }  

  private string _baseUrl = BASE_URL;
  private string _organisationPath = ORGANISATION_PATH;
}
