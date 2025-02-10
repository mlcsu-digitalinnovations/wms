using Newtonsoft.Json;
using System.Collections.Generic;

namespace WmsHub.Business.Models.MSGraph;

public class ElectiveCareGraphResponse
{
  [JsonProperty("value")]
  public List<FilteredUser> ElectiveCareUsers { get; set; }
  [JsonProperty("@odata.context")]
  public string OdataContext { get; set; }
}

