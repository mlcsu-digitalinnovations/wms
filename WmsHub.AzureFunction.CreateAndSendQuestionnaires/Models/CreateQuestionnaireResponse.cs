using System.Text.Json.Serialization;

namespace WmsHub.AzureFunction.CreateAndSendQuestionnaires.Models;

public class CreateQuestionnaireResponse
{
  public List<string> Errors { get; set; }
  [JsonRequired]
  public int NumberOfErrors { get; set; }
  [JsonRequired]
  public int NumberOfQuestionnairesCreated { get; set; }
}
