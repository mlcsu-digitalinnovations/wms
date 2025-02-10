using Newtonsoft.Json;

namespace WmsHub.Business.Models.MSGraph;

public class CreateUserPassword
{
  [JsonProperty("forceChangePasswordNextSignIn")]
  public bool ForceChangePasswordNextSignIn { get; set;}
  [JsonProperty("password")]
  public string Password { get; set; }
}
