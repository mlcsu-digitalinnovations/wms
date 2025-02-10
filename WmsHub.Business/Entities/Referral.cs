using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Entities;

public class Referral : ReferralBase, IReferral
{
  public virtual List<Call> Calls { get; set; } = [];
  public virtual Provider Provider { get; set; }
  public virtual List<TextMessage> TextMessages { get; set; } = [];
  public virtual List<ProviderSubmission> ProviderSubmissions { get; set; }
    = new List<ProviderSubmission>();
  public virtual ReferralCri Cri { get; set; }
  public virtual List<ReferralAudit> Audits { get; set; }
  public virtual ReferralQuestionnaire ReferralQuestionnaire { get; set; }

  internal void ResetToStatusNew()
  {
    ResetToAnyStatusBeforeProviderSelection();
    Status = ReferralStatus.New.ToString();
  }

  internal void ResetToStatusRmcCall()
  {
    ResetToAnyStatusBeforeProviderSelection();
    Status = ReferralStatus.RmcCall.ToString();
  }

  private void ResetToAnyStatusBeforeProviderSelection()
  {
    DateCompletedProgramme = null;
    DateLetterSent = null;
    DateOfProviderContactedServiceUser = null;
    DateOfProviderSelection = null;
    DateStartedProgramme = null;
    DateToDelayUntil = null;
    DelayReason = null;
    FirstRecordedWeight = null;
    FirstRecordedWeightDate = null;
    LastRecordedWeight = null;
    LastRecordedWeightDate = null;
    ProgrammeOutcome = null;
    ProviderId = null;
    ProviderSubmissions = null;
  }

  public Referral ShallowCopy() => (Referral)this.MemberwiseClone();
}
