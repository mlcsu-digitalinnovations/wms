using Newtonsoft.Json;
using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models;

public class CreateReferralQuestionnaireResponse
{
  public int NumberOfQuestionnairesCreated { get; set; } = 0;
  public int NumberOfErrors { get; set; } = 0;
  public List<string> Errors { get; set; } = new();
  [JsonIgnore]
  public CreateQuestionnaireStatus Status { get; set; } = 
    CreateQuestionnaireStatus.Valid;
  [JsonIgnore]
  public bool HasNoContent => 
    NumberOfQuestionnairesCreated == 0 && NumberOfErrors == 0;
}
