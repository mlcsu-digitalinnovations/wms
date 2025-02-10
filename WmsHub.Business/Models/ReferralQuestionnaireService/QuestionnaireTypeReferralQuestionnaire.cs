using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models;

public class QuestionnaireTypeReferralQuestionnaire
{
  public Guid Id { get; set; }
  public string GivenName { get; set; }
  public string FamilyName { get; set; }
  public string Email { get; set; }
  public string Mobile { get; set; }
  public string ProgrammeOutcome { get; set; }
  public string ReferralSource { get; set; }
  public string TriagedCompletionLevel { get; set; }
  public QuestionnaireType Type 
  {  
    get 
    { 
      return CalculateQuestionnaireType();
    }
  }

  private QuestionnaireType CalculateQuestionnaireType()
  {
    QuestionnaireType type;
    if (ProgrammeOutcome == Enums.ProgrammeOutcome.Complete.ToString())
    {
      if (ReferralSource == Enums.ReferralSource.SelfReferral.ToString())
      {
        type = TriagedCompletionLevel == Enums.TriageLevel.Low.ToString() ?
         QuestionnaireType.CompleteSelfT1 :
         QuestionnaireType.CompleteSelfT2and3;
      }
      else
      {
        type = TriagedCompletionLevel == Enums.TriageLevel.Low.ToString() ?
          QuestionnaireType.CompleteProT1 :
          QuestionnaireType.CompleteProT2and3;
      }
    }
    else
    {
      if (ReferralSource == Enums.ReferralSource.SelfReferral.ToString())
      {
        return QuestionnaireType.NotCompleteSelfT1and2and3;
      }
      else
      {
        return QuestionnaireType.NotCompleteProT1and2and3;
      }
    }

    return type;
  }
}
