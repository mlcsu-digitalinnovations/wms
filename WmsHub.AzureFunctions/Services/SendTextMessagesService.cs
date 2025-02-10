using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WmsHub.AzureFunctions.Exceptions;
using WmsHub.AzureFunctions.Options;

namespace WmsHub.AzureFunctions.Services;
internal class SendTextMessagesService(
  HttpClient httpClient,
  ILoggerFactory loggerFactory,
  IOptions<SendTextMessagesOptions> sendTextMessagesOptions)
  : ISendTextMessagesService
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly ILogger _logger = loggerFactory.CreateLogger<SendTextMessagesService>();
  private readonly SendTextMessagesOptions _options = sendTextMessagesOptions.Value;

  public async Task<string> ProcessAsync()
  {
    int checkedMessages = 0;
    int preparedMessages = await PrepareTextMessages();
    int totalSentMessages = 0;

    if (preparedMessages > 0)
    {
      checkedMessages = await CheckSendTextMessages();

      if (checkedMessages > 0)
      {
        int sendRetries = 0;

        while (totalSentMessages < checkedMessages && sendRetries < _options.MaxSendRetries)
        {
          int sentMessages = await SendTextMessages();

          if (sentMessages > 0)
          {
            totalSentMessages += sentMessages;
            sendRetries = 0;
          }
          else
          {
            sendRetries++;
          }
        }
      }
    }

    if (totalSentMessages < checkedMessages)
    {
      throw new SendTextMessagesException($"Checked: {checkedMessages}, Sent: {totalSentMessages}");
    }

    string result = $"Prepared: {preparedMessages}, Checked: {checkedMessages}, "
      + $"Sent: {totalSentMessages}";

    return result;
  }

  private async Task<int> CheckSendTextMessages()
  {
    _logger.LogDebug("Starting {MethodName}", nameof(CheckSendTextMessages));
    int result = await GetTextMessages(_options.CheckSendUrl);
    return result;
  }

  private async Task<int> PrepareTextMessages()
  {
    _logger.LogDebug("Starting {MethodName}", nameof(PrepareTextMessages));
    int result = await GetTextMessages(_options.PrepareUrl);
    return result;
  }

  private async Task<int> SendTextMessages()
  {
    _logger.LogDebug("Starting {MethodName}", nameof(SendTextMessages));
    int result = await GetTextMessages(_options.SendUrl);
    return result;
  }

  private async Task<int> GetTextMessages(string uri)
  {
    HttpResponseMessage responseMessage = await _httpClient.GetAsync(uri);

    if (responseMessage.StatusCode != System.Net.HttpStatusCode.OK)
    {
      throw new HttpRequestException(
        $"GET request to '{uri}' returned a status code of {responseMessage.StatusCode:d}.");
    }

    string responseString = await responseMessage.Content.ReadAsStringAsync();

    if (!int.TryParse(responseString, out int result))
    {
      throw new FormatException(
        $"GET request to '{uri}' returned non-integer content: {responseString}.");
    }

    return result;
  }
}
