using System.Text.Json.Serialization;
using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.Ui.Models;

public class EmailElectiveCareLinkPersonalisation : IEmailPersonalisation
{
  public static string[] ExpectedPersonalisation { get; } = 
  [
    NotificationPersonalisations.GIVEN_NAME,
    NotificationPersonalisations.LINK
  ];

  [JsonPropertyName("givenName")]
  public string GivenName { get; set; }

  [JsonPropertyName("link")]
  public string Link { get; set; }
}
