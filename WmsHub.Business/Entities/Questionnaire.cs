using Newtonsoft.Json;
using System.Collections.Generic;

namespace WmsHub.Business.Entities;

public class Questionnaire : QuestionnaireBase
{
  [JsonIgnore]
  public virtual List<ReferralQuestionnaire> ReferralQuestionnaires 
    { get; set; } = new List<ReferralQuestionnaire>();
}
