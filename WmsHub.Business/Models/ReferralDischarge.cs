using System;
using WmsHub.Business.Enums;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Models
{
  public class ReferralDischarge
  {
    public DateTimeOffset DateCompletedProgramme { get; set; }
    public Guid Id { get; set; }
    public decimal? LastRecordedWeight { get; set; }
    public DateTimeOffset? LastRecordedWeightDate { get; set; }
    public string ProgrammeOutcome { get; set; }
    public string ProviderName { get; set; }
    public string TriageLevel { get; set; }
    public string Ubrn { get; set; }
    public string NhsNumber { get; set; }
    public decimal? WeightOnReferral { get; set; }

    public override string ToString()
    {
      const string NA = "N/A";
      const string DWMP = "NHS Digital Weight Management Programme";
      string dischargeMessage = "";

      ProgrammeOutcome programmeOutcome = ProgrammeOutcome
        .ParseToEnumName<ProgrammeOutcome>();

      string lastRecordedWeightDateString = LastRecordedWeightDate.HasValue
        ? $"{LastRecordedWeightDate:dd/MMM/yyyy}"
        : NA;

      string weightOnReferralString = WeightOnReferral.HasValue
        ? $"{WeightOnReferral:#.00}"
        : NA;

      string lastRecordedWeightString = LastRecordedWeight.HasValue
        ? $"{LastRecordedWeight:#.00}"
        : NA;

      switch (programmeOutcome)
      {
        case Enums.ProgrammeOutcome.DidNotCommence:
          dischargeMessage = $"Did not commence {DWMP}. " +
            $"Date of discharge: {DateCompletedProgramme:dd/MMM/yyyy}.";
          break;
        case Enums.ProgrammeOutcome.DidNotComplete:
          dischargeMessage = $"Did not complete {DWMP}. " +
            $"Level of intervention triage: Level {TriageLevel}. " +
            $"Provider selected: {ProviderName}. " + 
            "Last date of engagement with the service: " +
            $"{DateCompletedProgramme:dd/MMM/yyyy}. " +
            $"Weight on referral: {weightOnReferralString}. " +
            $"Last recorded weight: {lastRecordedWeightString}. " +
            $"Last recorded weight date: {lastRecordedWeightDateString}.";
          break;
        case Enums.ProgrammeOutcome.Complete:
          dischargeMessage = $"{DWMP} completed. " +
            $"Level of intervention triage: Level {TriageLevel}. " +
            $"Provider selected: {ProviderName}. " +
            $"Date of completion: {DateCompletedProgramme:dd/MMM/yyyy}. " +
            $"Weight on referral: {weightOnReferralString}. " +
            $"Last recorded weight: {LastRecordedWeight}. " +
            $"Last recorded weight date: {lastRecordedWeightDateString}.";
          break;
      }

      return dischargeMessage;
    }
  }
}
