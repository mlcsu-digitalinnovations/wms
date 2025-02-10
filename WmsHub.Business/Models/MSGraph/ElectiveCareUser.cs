using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace WmsHub.Business.Models.MSGraph;
public class ElectiveCareUser: ElectiveCareUserBase
{


  [JsonProperty("businessPhones")]
  public List<string> BusinessPhones { get; set; }
  public List<string> Errors { get; set; }
  [JsonProperty("givenName")]
  public string GivenName { get; set; }
  [JsonProperty("jobTitle")]
  public string JobTitle { get; set; }
  [JsonProperty("mail")]
  public string Mail { get; set; }
  [JsonProperty("mobilePhone")]
  public string MobilePhone { get; set; }
  [JsonProperty("@odata.context")]
  public string OdataContext { get; set; }
  [JsonProperty("officeLocation")]
  public string OfficeLocation { get; set; }
  [JsonProperty("preferredLanguage")]
  public string PreferredLanguage { get; set; }
  [JsonProperty("surname")]
  public string Surname { get; set; }
  [JsonProperty("userPrincipalName")]
  public string UserPrincipalName { get; set; }

  public string ErrorMessage => string.Join(", ", Errors);
  public bool IsValid => Errors == null || !Errors.Any();


}

