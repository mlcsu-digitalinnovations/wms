using System.Net;
using System.Text;
using System.Text.Json;
using WmsHub.AzureFunction.CreateAndSendQuestionnaires.Models;
using static System.Net.Mime.MediaTypeNames;

namespace WmsHub.AzureFunction.CreateAndSendQuestionnaires.Tests;
public class CreateAndSendQuestionnairesTestData
{
  internal const string CreateQuestionnairesErrorMessage =
    $"POST to '{TestCreateQuestionnairesPath}' - ";
  internal const string ErrorText = "Error text";
  internal const int MaximumQuestionnaires = 250;
  internal const string SendQuestionnairesErrorMessage =
    $"POST to '{TestSendQuestionnairesPath}' - ";
  internal const string TestBaseUrl = "https://base/";
  internal const string TestCreateQuestionnairesPath = "CreateUrl";
  internal const int TestMaximumIterations = 3;
  internal const string TestReferralApiQuestionnaireKey = "ApiKey";
  internal const string TestSendQuestionnairesPath = "SendUrl";

  internal static string ConvertToString(object content) =>
    JsonSerializer.Serialize(content, content.GetType());

  internal static StringContent ConvertToStringContent(object content) =>
    new(ConvertToString(content), Encoding.UTF8, Application.Json);

  internal static CreateQuestionnaireResponse GetCreateQuestionnaireResponse(
    int numberOfQuestionnairesCreated,
    int numberOfErrors)
  {
    List<string> errors = [];

    for (int i = 0; i < numberOfErrors; i++)
    {
      errors.Add(ErrorText);
    }

    return new CreateQuestionnaireResponse()
    {
      NumberOfQuestionnairesCreated = numberOfQuestionnairesCreated,
      NumberOfErrors = numberOfErrors,
      Errors = errors
    };
  }

  internal static List<HttpResponseMessage> GetCreateQuestionnaireResponseList(int totalQuestionnaires)
  {
    List<HttpResponseMessage> responses = [];
    int numberOfRequests = GetNumberOfRequests(totalQuestionnaires);

    for (int i = 0; i < numberOfRequests; i++)
    {
      int batchSize = GetBatchSize(totalQuestionnaires, i);
      HttpResponseMessage response = new(HttpStatusCode.OK)
      {
        Content = ConvertToStringContent(GetCreateQuestionnaireResponse(batchSize, 0))
      };
      responses.Add(response);
    }

    responses.Add(new(HttpStatusCode.NoContent));
    return responses;
  }

  internal static SendQuestionnaireResponse GetSendQuestionnaireResponse(
    int numberOfQuestionnairesSent,
    int numberOfQuestionnairesFailed)
  {
    return new SendQuestionnaireResponse()
    {
      NumberOfReferralQuestionnairesSent = numberOfQuestionnairesSent,
      NumberOfReferralQuestionnairesFailed = numberOfQuestionnairesFailed,
      NoQuestionnairesToSend = false
    };
  }

  internal static List<HttpResponseMessage> GetSendQuestionnaireResponseList(
    int totalQuestionnaires)
  {
    List<HttpResponseMessage> responses = [];
    int numberOfRequests = GetNumberOfRequests(totalQuestionnaires);

    for (int i = 0; i < numberOfRequests; i++)
    {
      int batchSize = GetBatchSize(totalQuestionnaires, i);
      responses.Add(new(HttpStatusCode.OK)
      {
        Content = ConvertToStringContent(GetSendQuestionnaireResponse(batchSize, 0))
      });
    }

    responses.Add(new HttpResponseMessage(HttpStatusCode.OK)
    {
      Content = ConvertToStringContent(GetSendQuestionnaireResponse(0, 0))
    });

    return responses;
  }

  private static int GetBatchSize(int totalQuestionnaires, int i) =>
    Math.Min(MaximumQuestionnaires, totalQuestionnaires - (i * MaximumQuestionnaires));

  private static int GetNumberOfRequests(int totalQuestionnaires) =>
    (int)Math.Ceiling((double)totalQuestionnaires / MaximumQuestionnaires);
}
