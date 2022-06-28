using System;
using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ProviderService
{
  public class ProviderSubmissionRequest : IProviderSubmissionRequest
  {
    public ProviderSubmissionRequest(
      ServiceUserSubmissionRequest request, Guid userId, Guid referralId)
    {
      Ubrn = request.Ubrn;
      UpdateType = request.UpdateType;
      ReferralStatus = request.ReasonStatus;
      Reason = request.Reason;
      Date = request.Date ?? DateTimeOffset.Now.Date;
      Submissions = new List<Entities.ProviderSubmission>();

      if (ReferralStatus == Enums.ReferralStatus.ProviderCompleted)
      {
        ProgrammeOutcome = ReferralStatus.ToString();
        DateCompletedProgramme = Date;
      }
      else if (ReferralStatus == Enums.ReferralStatus.ProviderStarted)
      {
        DateStartedProgramme = Date;
      }
      else if (ReferralStatus == 
               Enums.ReferralStatus.ProviderContactedServiceUser)
      {
        DateOfProviderContactedServiceUser = Date;
      }

      foreach (var update in request.Updates)
      {
        Entities.ProviderSubmission s = new Entities.ProviderSubmission();
        s.Date = update.Date.Value;
        s.ModifiedAt = DateTimeOffset.Now;
        s.ModifiedByUserId = userId;
        s.ReferralId = referralId;
        if (update.Weight != null) s.Weight = update.Weight.Value;
        if (update.Measure != null) s.Measure = update.Measure.Value;
        if (update.Coaching != null) s.Coaching = update.Coaching.Value;
        s.IsActive = true;
        Submissions.Add(s);
      }
    }

    public List<Entities.ProviderSubmission> Submissions { get; set; }
    public DateTimeOffset Date { get; set; }
    public string Ubrn { get; set; }
    public UpdateType UpdateType { get; set; }
    public ReferralStatus? ReferralStatus { get; set; }
    public string Reason { get; set; }

    public string ProgrammeOutcome { get; private set; }
    public DateTimeOffset? DateCompletedProgramme { get; private set; }
    public DateTimeOffset? DateStartedProgramme { get; private set; }
    public DateTimeOffset? DateOfProviderContactedServiceUser { get; set; }
  }
}
