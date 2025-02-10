using System.Text.Json.Serialization;

namespace WmsHub.AzureFunction.CreateAndSendQuestionnaires.Models;

public class SendQuestionnaireResponse
{
  public bool NoQuestionnairesToSend { get; set; }
  [JsonRequired]
  public int NumberOfReferralQuestionnairesFailed { get; set; }
  [JsonRequired]
  public int NumberOfReferralQuestionnairesSent { get; set; }
}
