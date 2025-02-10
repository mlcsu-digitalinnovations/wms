using System;
using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models;

internal class CreateReferralQuestionnaire
{
  public Guid Id { get; set; }
  public string GivenName { get; set; }
  public string FamilyName { get; set; }
  public string Email { get; set; }
  public string Mobile { get; set; }
  public string ProgrammeOutcome { get; set; }
  public string ReferralSource { get; set; }
  public string TriagedCompletionLevel { get; set; }
  public List<string> GetQuestionnaireTypeErrors { get; private set; } = new();

  internal QuestionnaireType? GetQuestionnaireType()
  {
    QuestionnaireType? type = null;

    if (!Enum.TryParse(ProgrammeOutcome, out ProgrammeOutcome outcome))
    {
      GetQuestionnaireTypeErrors.Add($"Unknown ProgrammeOutcome " +
        $"{ProgrammeOutcome} for referral {Id}.");
    }

    if (!Enum.TryParse(ReferralSource, out ReferralSource source))
    {
      GetQuestionnaireTypeErrors.Add($"Unknown ReferralSource " +
        $"{ReferralSource} for referral {Id}.");
    }

    if (!Enum.TryParse(TriagedCompletionLevel, out TriageLevel triageLevel))
    {
      GetQuestionnaireTypeErrors.Add($"Unknown TriagedCompletionLevel " +
        $"{TriagedCompletionLevel} for referral {Id}.");
    }

    if (outcome == Enums.ProgrammeOutcome.Complete)
    {
      if (source == Enums.ReferralSource.GpReferral)
      {
        type = triageLevel == TriageLevel.Low
          ? QuestionnaireType.CompleteProT1
          : QuestionnaireType.CompleteProT2and3;
      }
      else if (source == Enums.ReferralSource.SelfReferral)
      {
        type = triageLevel == TriageLevel.Low
          ? QuestionnaireType.CompleteSelfT1
          : QuestionnaireType.CompleteSelfT2and3;
      }
      else
      {
        GetQuestionnaireTypeErrors.Add($"Unsupported ReferralSource " +
          $"{ReferralSource} for referral {Id}.");
      }
    }
    else
    {
      if (source == Enums.ReferralSource.GpReferral)
      {
        type = QuestionnaireType.NotCompleteProT1and2and3;
      }
      else if (source == Enums.ReferralSource.SelfReferral)
      {
        type = QuestionnaireType.NotCompleteSelfT1and2and3;
      }
      else
      {
        GetQuestionnaireTypeErrors.Add($"Unsupported ReferralSource " +
          $"{ReferralSource} for referral {Id}.");
      }
    }

    return type;
  }
}
