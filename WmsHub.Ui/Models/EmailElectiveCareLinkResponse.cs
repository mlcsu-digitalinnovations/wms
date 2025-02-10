using System.Text.Json.Serialization;

namespace WmsHub.Ui.Models;

public class EmailElectiveCareLinkResponse : EmailResponse
{
  [JsonPropertyName("personalisation")]
  public new EmailElectiveCareLinkPersonalisation Personalisation { get; set; }
}
