using System;
using System.Text.Json.Serialization;

namespace WmsHub.Ui.Models;

public class EmailResponse
{
  [JsonPropertyName("clientReference")]
  public string ClientReference { get; set; }

  [JsonPropertyName("email")]
  public string Email { get; set; }

  [JsonPropertyName("id")]
  public string Id { get; set; }

  [JsonPropertyName("personalisation")]
  public EmailProvidersListPersonalisation Personalisation { get; set; }

  [JsonPropertyName("senderId")]
  public string SenderId { get; set; }

  [JsonPropertyName("status")]
  public string Status { get; set; }

  [JsonPropertyName("statusDateTime")]
  public DateTime StatusDateTime { get; set; }

  [JsonPropertyName("templateId")]
  public string TemplateId { get; set; }
}
