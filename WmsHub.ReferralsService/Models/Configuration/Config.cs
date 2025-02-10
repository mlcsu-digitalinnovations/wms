using System.Diagnostics.CodeAnalysis;

namespace WmsHub.ReferralsService.Models.Configuration
{
  [ExcludeFromCodeCoverage]
  public class Config
  {
    public DataConfiguration Data { get; set; }

    /// <summary>
    /// The name of the named HttpClient with Client certificate.
    /// </summary>
    public const string HttpClientWithClientCertificate = "ClientCertificateHttpClient";

    /// <summary>
    /// The name of the named HttpClient for accessing the HubRegistration exception API.
    /// </summary>
    public const string HubRegistrationExceptionHttpClient = "HubRegistrationExceptionHttpClient";

    /// <summary>
    /// The name of the named HttpClient for accessing the HubRegistration API.
    /// </summary>
    public const string HubRegistrationHttpClient = "HubRegistrationHttpClient";
  }
}
