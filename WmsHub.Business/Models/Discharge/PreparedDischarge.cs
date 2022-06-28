using System;
using WmsHub.Business.Enums;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Models.Discharge
{
  public class PreparedDischarge
  {
    public const int NOT_SET = -1;
    private readonly DateTimeOffset? _dateCompletedProgramme;

    public static int DischargeAfterDays { get; set; } = NOT_SET;
    public static int DischargeCompletionDays { get; set; } = NOT_SET;
    public static decimal WeightChangeThreshold { get; set; } = NOT_SET;

    public Guid Id { get; private set; }
    public DateTimeOffset DateOfLastEngagement { get; private set; }
    public DateTimeOffset DateStartedProgramme { get; private set; }
    public decimal? FirstRecordedWeight => FirstWeightSubmission?.Weight;
    public DateTimeOffset? FirstRecordedWeightDate =>
      FirstWeightSubmission?.Date;
    public ProviderSubmission FirstWeightSubmission { get; private set; }
    public bool IsProgrammeOutcomeComplete =>
      ProgrammeOutcome == Enums.ProgrammeOutcome.Complete.ToString();
    public bool IsAwaitingDischarge =>
      Status == ReferralStatus.AwaitingDischarge.ToString();
    public bool IsDischargeOnHold =>
      Status == ReferralStatus.DischargeOnHold.ToString();
    public decimal? LastRecordedWeight => LastWeightSubmission?.Weight;
    public DateTimeOffset? LastRecordedWeightDate =>
      LastWeightSubmission?.Date;
    public ProviderSubmission LastWeightSubmission { get; private set; }
    public string ProgrammeOutcome { get; private set; }
    public ReferralSource ReferralSource { get; private set; }
    public string Status { get; private set; }
    public string StatusReason { get; private set; }
    public decimal WeightChange =>
      LastRecordedWeight - FirstRecordedWeight ?? 0;

    public PreparedDischarge(
      Guid id,
      DateTimeOffset dateStartedProgramme,
      DateTimeOffset? dateCompletedProgramme,
      ProviderSubmission firstWeightSubmission,
      ProviderSubmission lastWeightSubmission,
      DateTimeOffset dateOfLastEngagement,
      string referralSource)
    {
      if (DischargeAfterDays <= NOT_SET)
      {
        throw new InvalidOperationException(
          $"{nameof(DischargeAfterDays)} has not been set.");
      }

      if (DischargeCompletionDays <= NOT_SET)
      {
        throw new InvalidOperationException(
          $"{nameof(DischargeCompletionDays)} has not been set.");
      }

      if (WeightChangeThreshold <= NOT_SET)
      {
        throw new InvalidOperationException(
          $"{nameof(WeightChangeThreshold)} has not been set.");
      }


      Id = id == default
        ? throw new ArgumentException("Cannot be a default GUID", nameof(id))
        : id;

      _dateCompletedProgramme = 
        dateCompletedProgramme == DateTimeOffset.MinValue
          ? throw new ArgumentException(
              "Cannot be a default DateTimeOffset", 
              nameof(dateCompletedProgramme))
          : dateCompletedProgramme;

      DateStartedProgramme = dateStartedProgramme == default
        ? throw new ArgumentException(
            "Cannot be a default DateTimeOffset",
            nameof(dateStartedProgramme))
        : dateStartedProgramme;

      if (referralSource.TryParseToEnumName(out ReferralSource refSource))
      {
        ReferralSource = refSource;
      }
      else
      {
        throw new ArgumentException("Is not a valid referral source",
          nameof(referralSource));
      }

      DateOfLastEngagement = dateOfLastEngagement;

      FirstWeightSubmission = firstWeightSubmission;

      LastWeightSubmission = lastWeightSubmission;

      CalculateProgrammeOutcome();
      CalculateStatus();
    }

    private void CalculateProgrammeOutcome()
    {
      if (DateOfLastEngagement == default)
      {
        DateOfLastEngagement = _dateCompletedProgramme ?? 
          DateStartedProgramme.AddDays(DischargeAfterDays);
        ProgrammeOutcome = Enums.ProgrammeOutcome.DidNotCommence.ToString();
      }
      else
      {
        double lastSubmissionDays = 
          (DateOfLastEngagement.Date - DateStartedProgramme.Date)
            .TotalDays;

        if (lastSubmissionDays >= DischargeCompletionDays)
        {
          ProgrammeOutcome = Enums.ProgrammeOutcome.Complete.ToString();
        }
        else
        {
          ProgrammeOutcome = Enums.ProgrammeOutcome.DidNotComplete.ToString();
        }
      }
    }

    private void CalculateStatus()
    {
      if (ReferralSource == ReferralSource.GpReferral)
      {
        // put discharge on hold if the weight change is larger than the
        // expected threshold
        if (WeightChange > WeightChangeThreshold)
        {
          Status = ReferralStatus.DischargeOnHold.ToString();
          StatusReason = $"Weight gain of {WeightChange} is more than " +
            $"the expected maximum of {WeightChangeThreshold}.";
        }
        else if (WeightChange < -WeightChangeThreshold)
        {
          Status = ReferralStatus.DischargeOnHold.ToString();
          StatusReason = $"Weight loss of {WeightChange * -1} is more than " +
            $"the expected maximum of {WeightChangeThreshold}.";
        }
        else
        {
          Status = ReferralStatus.AwaitingDischarge.ToString();
        }        
      }
      else
      {
        Status = ReferralStatus.Complete.ToString();
      }
    }
  }
}
