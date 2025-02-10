using System;
using System.Collections.Generic;
using System.Linq;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Common.Attributes;

namespace WmsHub.Business.Models.Discharge;

public class PreparedDischarge
{
  public const int NOT_SET = -1;
  
  public DateTimeOffset DateOfLastEngagement { get; private set; }
  public DateTimeOffset? DateStartedProgramme { get; private set; }
  public decimal? FirstRecordedWeight => FirstWeightSubmission?.Weight;
  public DateTimeOffset? FirstRecordedWeightDate => FirstWeightSubmission?.Date;
  public ProviderSubmission FirstWeightSubmission { get; private set; }
  public Guid Id { get; private set; }
  public bool IsAwaitingDischarge => Status == ReferralStatus.AwaitingDischarge.ToString();
  public bool IsDischargeOnHold => Status == ReferralStatus.DischargeOnHold.ToString();
  public bool IsProgrammeOutcomeComplete =>
    ProgrammeOutcome == Enums.ProgrammeOutcome.Complete.ToString();
  public bool IsUnableToDischarge => Status == ReferralStatus.UnableToDischarge.ToString();
  public decimal? LastRecordedWeight { get; private set; }
  public DateTimeOffset? LastRecordedWeightDate { get; private set; }
  public ProviderSubmission LastWeightSubmission { get; private set; }
  public string NhsNumber { get; private set; }
  public string ProgrammeOutcome { get; private set; }
  public string ReferringGpPracticeNumber { get; private set; }
  public string Status { get; set; }
  public string StatusReason { get; private set; }
  public decimal WeightChange => LastWeightSubmission?.Weight - FirstRecordedWeight ?? 0;

  private readonly DateTimeOffset? _dateCompletedProgramme;
  private readonly DateTimeOffset _dateOfProviderSelection;
  private readonly GpPracticeOdsCodeAttribute _gpPracticeCodeValidator =
    new(allowDefaultCodes: false);
  private readonly NhsNumberAttribute _nhsNumberValidator = new(allowNulls: false);

  private static int DischargeAfterDays { get; set; } = NOT_SET;
  private static int DischargeCompletionDays { get; set; } = NOT_SET;
  public static int TerminateAfterDays { get; set; } = NOT_SET;
  private static decimal WeightChangeThreshold { get; set; } = NOT_SET;

  public PreparedDischarge(
    Guid id,
    DateTimeOffset dateOfProviderSelection,
    DateTimeOffset? dateStartedProgramme,
    DateTimeOffset? dateCompletedProgramme,
    string nhsNumber,
    List<Entities.ProviderSubmission> providerSubmissions,
    string referringGpPracticeNumber)
  {
    AssertOptionsHaveBeenSet();    

    _dateCompletedProgramme = dateCompletedProgramme == DateTimeOffset.MinValue
      ? throw new ArgumentException(
          "Cannot be a default DateTimeOffset", 
          nameof(dateCompletedProgramme))
      : dateCompletedProgramme;

    _dateOfProviderSelection = dateOfProviderSelection == default
      ? throw new ArgumentException(
          "Cannot be a default DateTimeOffset",
          nameof(dateOfProviderSelection))
      : dateOfProviderSelection;

    DateStartedProgramme = dateStartedProgramme == DateTimeOffset.MinValue
      ? throw new ArgumentException(
          "Cannot be a default DateTimeOffset", 
          nameof(dateStartedProgramme))
      : dateStartedProgramme;

    Id = id == default ? throw new ArgumentException("Cannot be a default GUID", nameof(id)) : id;

    DateOfLastEngagement = providerSubmissions
      .OrderByDescending(ps => ps.Date)
      .Select(ps => ps.Date)
      .FirstOrDefault();

    FirstWeightSubmission = providerSubmissions
      .Where(ps => ps.Weight > 0)
      .OrderBy(ps => ps.Date.Date)
        .ThenByDescending(ps => ps.ModifiedAt)
      .Select(ps => new ProviderSubmission
      {
        Date = ps.Date,
        Weight = ps.Weight
      })
      .FirstOrDefault();

    LastWeightSubmission = providerSubmissions
      .Where(ps => ps.Weight > 0)
      .OrderByDescending(ps => ps.Date.Date)
        .ThenByDescending(ps => ps.ModifiedAt)
      .Select(ps => new ProviderSubmission
      {
        Date = ps.Date,
        Weight = ps.Weight
      })
      .FirstOrDefault();

    LastRecordedWeight = LastWeightSubmission?.Weight ?? null;
    LastRecordedWeightDate = LastWeightSubmission?.Date ?? null;

    NhsNumber = nhsNumber;
    ReferringGpPracticeNumber = referringGpPracticeNumber;

    CalculateProgrammeOutcome();
    CalculateStatus();
  }

  public static void AssertOptionsHaveBeenSet()
  {
    if (DischargeAfterDays <= NOT_SET)
    {
      throw new InvalidOperationException($"{nameof(DischargeAfterDays)} has not been set.");
    }

    if (DischargeCompletionDays <= NOT_SET)
    {
      throw new InvalidOperationException($"{nameof(DischargeCompletionDays)} has not been set.");
    }

    if (TerminateAfterDays <= NOT_SET)
    {
      throw new InvalidOperationException($"{nameof(TerminateAfterDays)} has not been set.");
    }

    if (WeightChangeThreshold <= NOT_SET)
    {
      throw new InvalidOperationException($"{nameof(WeightChangeThreshold)} has not been set.");
    }
  }

  public static void SetOptions(
    ProviderOptions providerOptions,
    ReferralTimelineOptions referralTimeline)
  {
    DischargeCompletionDays = providerOptions.DischargeCompletionDays;
    DischargeAfterDays = providerOptions.DischargeAfterDays;
    TerminateAfterDays = referralTimeline.MaxDaysToStartProgrammeAfterProviderSelection;
    WeightChangeThreshold = providerOptions.WeightChangeThreshold;
  }

  private void CalculateProgrammeOutcome()
  {
    if (DateStartedProgramme.HasValue)
    {
      if (DateOfLastEngagement == default)
      {
        DateOfLastEngagement = _dateCompletedProgramme ??
          DateStartedProgramme.Value.AddDays(DischargeAfterDays);
        ProgrammeOutcome = Enums.ProgrammeOutcome.DidNotCommence.ToString();
      }
      else
      {
        double lastSubmissionDays =
          (DateOfLastEngagement.Date - DateStartedProgramme.Value.Date)
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
    else
    {
      DateOfLastEngagement = _dateOfProviderSelection.AddDays(TerminateAfterDays);
      ProgrammeOutcome = Enums.ProgrammeOutcome.DidNotCommence.ToString();
    }
  }

  private void CalculateStatus()
  {
    decimal absoluteWeightChange = Math.Abs(WeightChange);
    if (absoluteWeightChange > WeightChangeThreshold)
    {
      LastRecordedWeight = null;
      LastRecordedWeightDate = null;
      StatusReason = $"Weight {(WeightChange > WeightChangeThreshold ? "gain" : "loss")} of " +
        $"{absoluteWeightChange} is more than the expected maximum of {WeightChangeThreshold}.";
    }

    bool isNhsNumberValid = _nhsNumberValidator.IsValid(NhsNumber);
    bool isReferringGpPracticeNumberValid = ReferringGpPracticeNumber is not null
      && _gpPracticeCodeValidator.IsValid(ReferringGpPracticeNumber);

    if (isNhsNumberValid == false || isReferringGpPracticeNumberValid == false)
    {
      string statusReason = "";
      if (isNhsNumberValid == false)
      {
        statusReason = $"{nameof(NhsNumber)} is invalid.";
      }
      if (isReferringGpPracticeNumberValid == false) 
      {
        statusReason += $" {nameof(ReferringGpPracticeNumber)} is invalid.";
      }

      Status = ReferralStatus.UnableToDischarge.ToString();
      StatusReason = statusReason.Trim();
    }
    else
    {
      Status = ReferralStatus.AwaitingDischarge.ToString();
    }
  }
}
