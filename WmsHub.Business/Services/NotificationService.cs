using System;
using System.Net.Http;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using System.Net;
using System.Linq;
using WmsHub.Business.Enums;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Serilog;
using WmsHub.Business.Models.Notify;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Options;
using WmsHub.Business.Exceptions;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Services;
public class NotificationService :
  ServiceBase<Entities.TextMessage>, INotificationService
{
  private const string HEADER_X_API_KEY = "x-api-key";

  private readonly ILogger _logger;
  private readonly HttpClient _httpClient;
  private readonly NotificationOptions _options;

  public NotificationService(
    DatabaseContext context,
    ILogger logger,
    HttpClient httpClient,
    IOptions<NotificationOptions> options
  ): base(context)
  {
    if (context== null)
    {
      throw new ArgumentNullException($"{nameof(context)} is null.");
    }

    _logger = logger 
      ?? throw new ArgumentNullException($"{nameof(logger)} is null.");
    _httpClient = httpClient 
      ?? throw new ArgumentNullException($"{nameof(HttpClient)} is null.");

    _options = options == null
      ? throw new ArgumentNullException(
        $"{nameof(IOptions<NotificationOptions>)} is null.")
      : options.Value == null
        ? throw new ArgumentNullException(
          $"{nameof(NotificationOptions)} is null.")
        : options.Value;
  }

  public virtual async Task<HttpResponseMessage> GetEmailHistory(string clientReference)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(clientReference);

    _httpClient.DefaultRequestHeaders.Add(HEADER_X_API_KEY, _options.NotificationApiKey);
    string url = $"{_options.NotificationApiUrl}/email?clientReference={clientReference}";
    HttpResponseMessage response = await _httpClient.GetAsync(url);

    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
    {
      return response;
    }
    else if (response.StatusCode == HttpStatusCode.BadRequest
      || response.StatusCode == HttpStatusCode.Forbidden)
    {
      string error = await response.Content.ReadAsStringAsync();
      _logger.Error(error);
      throw new NotificationProxyException(error);
    }
    else
    {
      _logger.Error(response.ReasonPhrase);
      throw new NotificationProxyException(response.ReasonPhrase);
    }
  }

  [Obsolete("User the generic SendMessageAsync method which can send " +
    "either Text or Email messages.")]
  public virtual async Task<SmsPostResponse> SendNotificationAsync(
    SmsPostRequest request)
  {
    if (request == null)
    {
      throw new ArgumentNullException($"{nameof(request)} is null");
    }

    ValidateModelResult validationResult = ValidateModel(request);

    if (!validationResult.IsValid)
    {
      throw new ValidationException(
        string.Join(" ", 
        validationResult.Results.Select(s => s.ErrorMessage).ToArray()));
    }

    _httpClient.DefaultRequestHeaders
      .Add(HEADER_X_API_KEY, _options.NotificationApiKey);

    HttpContent content = new StringContent(
      JsonConvert.SerializeObject(request),
      Encoding.UTF8,
      MediaTypeNames.Application.Json);

    HttpResponseMessage notificationResponse = await _httpClient
      .PostAsync(_options.NotificationUrl, content);

    string contentAsString =
      await notificationResponse.Content.ReadAsStringAsync();

    return notificationResponse.StatusCode switch
    {
      HttpStatusCode.Created =>
        JsonConvert.DeserializeObject<SmsPostResponse>(contentAsString),
      HttpStatusCode.BadRequest =>
        GetResponseModel(contentAsString, request),
      HttpStatusCode.ServiceUnavailable => GetResponseModel(
          contentAsString,
          request,
          ReferralQuestionnaireStatus.TechnicalFailure),
      HttpStatusCode.TooManyRequests => new SmsPostResponse
      {
        ClientReference = request.ClientReference,
        Status = ReferralQuestionnaireStatus
                .TechnicalFailure.ToString()
      },
      _ => new()
      {
        ClientReference = request.ClientReference,
        Status = ReferralQuestionnaireStatus
          .TechnicalFailure.ToString(),
        GetNotificationErrors = new() 
        { 
          notificationResponse.StatusCode.ToString() 
        }
      },
    };
  }

  private static SmsPostResponse GetResponseModel(
    string content,
    SmsPostRequest request,
    ReferralQuestionnaireStatus status = 
      ReferralQuestionnaireStatus.PermanentFailure
  )
  {
    SmsPostErrorResponse errorResponse = JsonConvert
      .DeserializeObject<SmsPostErrorResponse>(content);

    SmsPostResponse response = new()
    {
      ClientReference = request.ClientReference,
      Status = status.ToString()
    };

    foreach (string key in errorResponse.Errors.Keys)
    {
      if (errorResponse.Errors[key] != null && errorResponse.Errors[key].Any())
      {
        response.GetNotificationErrors.AddRange(errorResponse.Errors[key]);
      }
    }

    return response;
  }

  public virtual async Task<HttpResponseMessage> GetMessageVerification(
    string messageId)
  {
    if (string.IsNullOrWhiteSpace(messageId))
    {
      throw new ArgumentException(
        $"{nameof(messageId)} cannot be null or empty.");
    }

    _httpClient.DefaultRequestHeaders.Add(
      HEADER_X_API_KEY,
      _options.NotificationApiKey);
    string url = $"{_options.NotificationUrl}/{messageId}";
    HttpResponseMessage response = await _httpClient.GetAsync(url);

    if (response.IsSuccessStatusCode)
    {
      return response;
    }
    else if (response.StatusCode == HttpStatusCode.BadRequest
      || response.StatusCode == HttpStatusCode.Forbidden)
    {
      string error = await response.Content.ReadAsStringAsync();
      _logger.Error(error);
      throw new NotificationProxyException(error);
    }
    else
    {
      _logger.Error(response.ReasonPhrase);
      throw new NotificationProxyException(response.ReasonPhrase);
    }
  }

  public virtual async Task<HttpResponseMessage> SendMessageAsync(MessageQueue message)
  {
    ValidateModelResult validationResult = ValidateModel(message);

    if (!validationResult.IsValid)
    {
      throw new ValidationException(
        string.Join(" ",
        validationResult.Results.Select(s => s.ErrorMessage).ToArray()));
    }

    _httpClient.DefaultRequestHeaders.Add(
      HEADER_X_API_KEY, message.ApiKeyType switch
      {
        ApiKeyType.FailedToContact => _options.FailedContactApiKey,
        ApiKeyType.ElectiveCareNewUser => _options.NotificationApiKey,
        ApiKeyType.ProviderList => _options.NotificationApiKey,
        ApiKeyType.TextMessage1 => _options.FailedContactApiKey,
        ApiKeyType.TextMessage2 => _options.FailedContactApiKey,
        _ => _options.NotificationApiKey
      }
    );

    if (string.IsNullOrWhiteSpace(message.SenderId))
    {
      message.SenderId = _options.NotificationSenderId;
    }

    _options.Endpoint = message.Endpoint;
    HttpResponseMessage response = await _httpClient.PostAsync(
      _options.NotificationUrl, 
      message.Content);

    if (response.IsSuccessStatusCode)
    {
      return response;
    }
    else if (response.StatusCode == HttpStatusCode.BadRequest
      || response.StatusCode == HttpStatusCode.Forbidden)
    {
      string error = await response.Content.ReadAsStringAsync();
      _logger.Error(error);
      throw new NotificationProxyException(error);
    }
    else
    {
      _logger.Error(response.ReasonPhrase);
      throw new NotificationProxyException(response.ReasonPhrase);
    }
  }
}
