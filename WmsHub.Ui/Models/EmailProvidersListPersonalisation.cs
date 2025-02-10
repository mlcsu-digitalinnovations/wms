using System.Text.Json.Serialization;
using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.Ui.Models;

public class EmailProvidersListPersonalisation : IEmailPersonalisation
{
  public static string[] ExpectedPersonalisation { get; } =
  [
    NotificationPersonalisations.GIVEN_NAME,
    NotificationPersonalisations.PROVIDER_COUNT,
    NotificationPersonalisations.PROVIDER_LIST,
    NotificationPersonalisations.LINK
  ];

  [JsonPropertyName("givenName")]
  public string GivenName { get; set; }

  [JsonPropertyName("providerCount")]
  public string ProviderCount { get; set; }

  [JsonPropertyName("providerList")]
  public string ProviderList { get; set; }

  [JsonPropertyName("link")]
  public string Link { get; set; }
}
