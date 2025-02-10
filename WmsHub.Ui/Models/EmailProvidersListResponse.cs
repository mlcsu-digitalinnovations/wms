using System.Text.Json.Serialization;

namespace WmsHub.Ui.Models;

public class EmailProvidersListResponse : EmailResponse
{
  [JsonPropertyName("personalisation")]
  public new EmailProvidersListPersonalisation Personalisation { get; set; }
}
