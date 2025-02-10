
using System.Text.Json.Serialization;

namespace WmsHub.Business.Models.AuthService
{
  public class AccessTokenResponse:BaseValidationResponse
  {
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
    [JsonPropertyName("expires")]
    public int Expires { get; set; }
  }
}


