using Newtonsoft.Json;

namespace WmsHub.Business.Models.MSGraph;

public class CreateIdentities
{
  /// <summary>
  /// From the MSGraph Options Issuer.
  /// </summary>
  [JsonProperty("issuer")]
  public string Issuer { get; set; }
  /// <summary>
  /// Email Address of user being added.
  /// </summary>
  [JsonProperty("issuerAssignedId")]
  public string IssuerAssignedId { get; set; }
  /// <summary>
  /// Choice of userName, emailAddress or federated<br />
  /// emailAddress is the default.
  /// </summary>
  [JsonProperty("signInType")]
  public string SignInType { get; set; } = "emailAddress";
}
