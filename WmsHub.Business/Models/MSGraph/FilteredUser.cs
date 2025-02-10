using Newtonsoft.Json;

namespace WmsHub.Business.Models.MSGraph;

public class FilteredUser: ElectiveCareUserBase
{
  /// <summary>
  /// GET https://graph.microsoft.com/v1.0/users?
  /// $select={string.join(",", list)},
  /// &$filter=identities/any(c:c/issuerAssignedId eq {email} and c/issuer
  /// {issuer})
  /// <list type="bullet">
  /// <item><description>{0} string Endpoint</description></item>
  /// <item><description>{1} string ApiVersion</description></item>
  /// <item><description>{2} string[] columns</description></item>
  /// <item><description>{3} string email</description></item>
  /// <item><description>{4} string issuer</description></item>
  /// </list>
  /// </summary>
  [JsonIgnore]
  public const string ENDPOINT = "{0}/{1}/users?$select={2}" +
    "&$filter=identities/any(c:c/issuerAssignedId eq '{3}' " +
    "and c/issuer eq '{4}')";
  [JsonProperty("extension_7f1f413c66724a3eabceb6cb6d43e063_ODS")]
  public string OdsCode { get; set; }
}
