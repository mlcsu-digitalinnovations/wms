using Newtonsoft.Json;
using System;

namespace WmsHub.Business.Models.MSGraph;

public class ElectiveCareUserBase
{
  [JsonProperty("displayName")]
  public string DisplayName { get; set; }
  [JsonProperty("id")]
  public Guid Id { get; set; }
  [JsonProperty("identities")]
  public virtual CreateIdentities[] Identities { get; set; }
}
