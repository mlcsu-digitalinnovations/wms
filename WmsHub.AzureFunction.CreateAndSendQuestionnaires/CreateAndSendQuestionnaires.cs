using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Serilog;
using System.Data;
using System.Text;
using System.Text.Json;
using WmsHub.AzureFunction.CreateAndSendQuestionnaires.Exceptions;
using WmsHub.AzureFunction.CreateAndSendQuestionnaires.Models;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WmsHub.AzureFunction.CreateAndSendQuestionnaires;

public class CreateAndSendQuestionnaires(
  IHttpClientFactory httpClientFactory,
  ILogger logger,
  IOptions<CreateAndSendQuestionnairesOptions> options,
  IProcessStatusService processStatusService)
{
  private const string HeaderXApiKeyName = "x-api-key";
  private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
  {
    PropertyNameCaseInsensitive = true
  };

  private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
  private HttpClient _referralApiHttpClient;
  private bool _isRetryRequired;
  private readonly ILogger _logger = logger?.ForContext<CreateAndSendQuestionnaires>();
  private int _numberOfIterations;
  private readonly CreateAndSendQuestionnairesOptions _options = options?.Value;
  private readonly IProcessStatusService _processStatusService = processStatusService;
  private int _totalQuestionnairesFailed;
  private int _totalQuestionnairesSent;

  /// <summary>
  /// Run Create and Send Questionnaire Process. Creates questionnaires for DWMP and sends them
  /// to service users.
  /// </summary>
  /// <param name="timer">Run Azure function at 12:00 Monday - Friday</param>
  /// <returns></returns>
  [Function("CreateAndSendQuestionnaires")]
  public async Task Run([TimerTrigger("0 0 12 * * 1-5", RunOnStartup = false)] Models.Timer timer)
  {
    _referralApiHttpClient = _httpClientFactory.CreateClient();
    _referralApiHttpClient.BaseAddress = new(_options.ReferralApiBaseUrl);
    _referralApiHttpClient.DefaultRequestHeaders
      .Add("x-api-key", _options.ReferralApiQuestionnaireKey);

    if (timer.IsPastDue)
    {
      _logger.Information(
        "{AzureFunctionName} Azure Function executed late.",
        nameof(CreateAndSendQuestionnaires));
    }

    try
    {
      await _processStatusService.StartedAsync();
      do
      {
        _numberOfIterations++;

        await CreateQuestionnairesAsync();
        await SendQuestionnairesAsync();
      }
      while (_isRetryRequired && (_numberOfIterations <= _options.MaximumIterations));

      string processCompleteMessage =
        $"Sent questionnaires: {_totalQuestionnairesSent}. " +
        $"Failed questionnaires: {_totalQuestionnairesFailed}. " +
        $"Iterations: {_numberOfIterations}";

      _logger.Information(processCompleteMessage);

      if (_numberOfIterations <= _options.MaximumIterations)
      {
        await _processStatusService.SuccessAsync(processCompleteMessage);
      }
      else
      {
        throw new ConstraintException(
          $"Exceeded max iterations of '{_options.MaximumIterations}'. {processCompleteMessage}");
      }
    }
    catch (Exception ex)
    {
      if (ex is not CreateQuestionnairesException
        && ex is not SendQuestionnairesException)
      {
        _logger.Error(ex, "An error occurred during the process.");
      }

      await _processStatusService.FailureAsync(ex.Message);
    }
  }

  private async Task CreateQuestionnairesAsync()
  {
    CreateQuestionnaireRequest createQuestionnaireRequest = new()
    {
      FromDate = _options.CreateQuestionnairesFromDate,
      MaxNumberToCreate = _options.MaximumQuestionnairesToCreate,
      ToDate = DateTimeOffset.Now
    };

    StringContent createQuestionnairesRequestStringContent = new(
      JsonSerializer.Serialize(createQuestionnaireRequest),
      Encoding.UTF8,
      Application.Json);

    HttpResponseMessage response = await _referralApiHttpClient.PostAsync(
      _options.CreateQuestionnairesPath,
      createQuestionnairesRequestStringContent);

    string responseString = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
      _logger.Error("POST to '{Url}' - '{StatusCode}': '{Response}'.",
        _options.CreateQuestionnairesPath,
        response.StatusCode,
        responseString);
      throw new CreateQuestionnairesException(
        $"POST to '{_options.CreateQuestionnairesPath}' - '{response.StatusCode}': " +
        $"'{responseString}'.");
    }

    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
    {
      _logger.Debug("Zero questionnaires created.");
      return;
    }

    CreateQuestionnaireResponse createQuestionnaireResponse = JsonSerializer
      .Deserialize<CreateQuestionnaireResponse>(responseString, s_jsonSerializerOptions);

    _logger.Information("Number of questionnaires created: {NumberOfQuestionnairesCreated}.",
      createQuestionnaireResponse.NumberOfQuestionnairesCreated);

    if (createQuestionnaireResponse.NumberOfErrors > 0)
    {
      string concatenatedErrors = string.Join(", ", createQuestionnaireResponse.Errors);

      _logger.Error(
        "POST to '{Url}' - {NumberOfErrors} errors: '{Errors}'.",
        _options.CreateQuestionnairesPath,
        createQuestionnaireResponse.NumberOfErrors,
        concatenatedErrors);

      throw new CreateQuestionnairesException(
        $"POST to '{_options.CreateQuestionnairesPath}' - " +
        $"{createQuestionnaireResponse.NumberOfErrors} errors: '{concatenatedErrors}'.");
    }

    return;
  }

  private async Task SendQuestionnairesAsync()
  {
    HttpResponseMessage response = await _referralApiHttpClient
      .PostAsync(_options.SendQuestionnairesPath, null);

    string responseString = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
      _logger.Error(
        "POST to '{Url}' - '{StatusCode}': '{Response}'.",
        _options.SendQuestionnairesPath,
        response.StatusCode,
        responseString);

      throw new SendQuestionnairesException(
        $"POST to '{_options.SendQuestionnairesPath}' - '{response.StatusCode}': '{responseString}'.");
    }

    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
    {
      _logger.Information("Zero questionnaires to send.");
      _isRetryRequired = false;
    }
    else
    {
      SendQuestionnaireResponse sendResponse = JsonSerializer
        .Deserialize<SendQuestionnaireResponse>(responseString, s_jsonSerializerOptions);

      _logger.Debug("Number of questionnaires sent: {QuestionnairesSent}.",
        sendResponse.NumberOfReferralQuestionnairesSent);
      _logger.Debug("Number of questionnaires failed: {QuestionnairesFailed}.",
        sendResponse.NumberOfReferralQuestionnairesFailed);

      _isRetryRequired = sendResponse.NumberOfReferralQuestionnairesSent > 0;
      _totalQuestionnairesFailed += sendResponse.NumberOfReferralQuestionnairesFailed;
      _totalQuestionnairesSent += sendResponse.NumberOfReferralQuestionnairesSent;
    }
  }
}
