using System.Text.Json.Serialization;

namespace WmsHub.Business.Entities;

public class ReferralQuestionnaire : ReferralQuestionnaireBase
{
  [JsonIgnore]
  public virtual Referral Referral { get; set; }
  [JsonIgnore]
  public virtual Questionnaire Questionnaire { get; set; }
}
