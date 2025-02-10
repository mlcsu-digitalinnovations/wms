using System.ComponentModel.DataAnnotations;

namespace WmsHub.AzureFunctions.Options;
public class SendTextMessagesOptions
{
  public static string SectionKey => nameof(SendTextMessagesOptions);

  [Required]
  public required string ApiKey { get; set; }

  [Required]
  public int BatchSize { get; set; } = 200;

  [Required]
  public string CheckSendEndpoint { get; set; } = "checksend";
  public string CheckSendUrl => $"{TextMessageApiUrl}/{CheckSendEndpoint}";

  [Required]
  public string FunctionName { get; set; } = "WmsHub.AzureFunctions.SendTextMessages.Daily";

  [Required]
  public int MaxSendRetries { get; set; } = 5;

  [Required]
  public string PrepareEndpoint { get; set; } = "prepare";
  public string PrepareUrl => $"{TextMessageApiUrl}/{PrepareEndpoint}";

  [Required]
  public string SendEndpoint { get; set; } = "send";
  [Required]
  public string SendQueryParameterBatchSizeLimit { get; set; } = "limit";
  public string SendUrl => 
    $"{TextMessageApiUrl}/{SendEndpoint}?{SendQueryParameterBatchSizeLimit}={BatchSize}";

  [Required]
  public required string TextMessageApiUrl { get; set; }
}
