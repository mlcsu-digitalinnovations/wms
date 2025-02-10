using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models;

public class CompleteQuestionnaireResponse
{
  public ReferralQuestionnaireStatus Status { get; set; }
  public QuestionnaireType QuestionnaireType { get; set; }
  public ReferralQuestionnaireValidationState ValidationState { get; set; }
  public List<string> GetQuestionnaireTypeErrors { get; private set; } = new();
}
