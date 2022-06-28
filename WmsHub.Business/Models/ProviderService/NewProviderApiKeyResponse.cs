using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ProviderService
{
  public class NewProviderApiKeyResponse : ProviderRequest, 
    INewProviderApiKeyResponse
  {
   public string ApiKey { get; set; }

    public DateTimeOffset ApiKeyExpiry { get; set; }

    /// <summary>
    /// using the [JsonIgnore] from System.Text.Json.Serialization works. 
    /// (if it used from NewtonSoft.Json, it doesn't!
    /// </summary>
    [JsonIgnore]
    public virtual StatusType ResponseStatus { get; set; }

    /// <summary>
    /// using the [JsonIgnore] from System.Text.Json.Serialization works. 
    /// (if it used from NewtonSoft.Json, it doesn't!
    /// </summary>
    [JsonIgnore]
    public virtual List<string> Errors { get; private set; }
      = new List<string>();

    public string GetErrorMessage()
    {
      string msg = string.Join(" ", Errors);
      return msg;
    }
  }
}
