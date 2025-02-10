using System.Text.Json.Serialization;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models;

public class StartReferralQuestionnaire
{
  public string FamilyName { get; set; }

  public string GivenName { get; set; }   
  
  public string ProviderName { get; set; }

  public QuestionnaireType QuestionnaireType { get; set; }

  [JsonIgnore]
  public ReferralQuestionnaireStatus Status { get; set; }
  [JsonIgnore]
  public QuestionnaireType Type { get; set; }
  [JsonIgnore]
  public ReferralQuestionnaireValidationState ValidationState { get; set; }
}
