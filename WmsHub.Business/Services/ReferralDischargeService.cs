using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Discharge;
using static WmsHub.Business.Enums.ReferralStatus;

namespace WmsHub.Business.Services
{
  public class ReferralDischargeService
    : ServiceBase<Entities.Referral>, IReferralDischargeService
  {
    private static bool IsPrepareDischargesInProgress = false;
    private readonly ProviderOptions _providerOptions;

    public ReferralDischargeService(
      DatabaseContext context,
      IOptions<ProviderOptions> providerOptions)
      : base(context)
    {
      _providerOptions = providerOptions.Value;
      PreparedDischarge.DischargeCompletionDays =
        _providerOptions.DischargeCompletionDays;
      PreparedDischarge.DischargeAfterDays =
        _providerOptions.DischargeAfterDays;
      PreparedDischarge.WeightChangeThreshold =
        _providerOptions.WeightChangeThreshold;
    }

    public async Task<string> PrepareDischarges()
    {
      if (IsPrepareDischargesInProgress)
      {
        return "PrepareDischarges is already in progress. " +
          "Please wait for the previous process to complete.";
      }
      string result = null;
      try
      {
        IsPrepareDischargesInProgress = true;

        List<PreparedDischarge> preparedDischarges = 
          await GetPreparedDischargesAsync();

        foreach (PreparedDischarge preparedDischarge in preparedDischarges)
        {
          Entities.Referral referral = await _context.Referrals
            .FindAsync(preparedDischarge.Id);

          referral.DateCompletedProgramme =
            preparedDischarge.DateOfLastEngagement;
          referral.FirstRecordedWeight = preparedDischarge.FirstRecordedWeight;
          referral.FirstRecordedWeightDate = preparedDischarge
            .FirstRecordedWeightDate;
          referral.LastRecordedWeight = preparedDischarge.LastRecordedWeight;
          referral.LastRecordedWeightDate = preparedDischarge
            .LastRecordedWeightDate;
          referral.ProgrammeOutcome = preparedDischarge.ProgrammeOutcome;
          referral.Status = preparedDischarge.Status;

          UpdateModified(referral);
        }
        await _context.SaveChangesAsync();

        result = GetResult(preparedDischarges);
      }
      finally
      {
        IsPrepareDischargesInProgress = false;
      }
      return result;

      async Task<List<PreparedDischarge>> GetPreparedDischargesAsync()
      {
        DateTimeOffset dischargeDate = DateTimeOffset.Now
          .AddDays(-_providerOptions.DischargeAfterDays);

        return await _context.Referrals
          .AsNoTracking()
          .Where(r => r.IsActive)
          .Where(r => r.DateStartedProgramme != null)
          .Where(r => r.DateStartedProgramme < dischargeDate)
          .Where(r => r.Status == ProviderCompleted.ToString() ||
            r.Status == ProviderContactedServiceUser.ToString() ||
            r.Status == ProviderDeclinedByServiceUser.ToString() ||
            r.Status == ProviderRejected.ToString() ||
            r.Status == ProviderStarted.ToString() ||
            r.Status == ProviderTerminated.ToString())
          .Select(r => new PreparedDischarge(
            r.Id,
            r.DateStartedProgramme.Value,
            r.DateCompletedProgramme,
            r.ProviderSubmissions
              .Where(ps => ps.Weight > 0)
              .OrderBy(ps => ps.Date.Date)
                .ThenByDescending(ps => ps.ModifiedAt)
              .Select(ps => new ProviderSubmission
              {
                Date = ps.Date,
                Weight = ps.Weight
              })
              .FirstOrDefault(),
            r.ProviderSubmissions
              .Where(ps => ps.Weight > 0)
              .OrderBy(r => r.Date.Date)
                .ThenBy(ps => ps.ModifiedAt)
              .Select(ps => new ProviderSubmission
              {
                Date = ps.Date,
                Weight = ps.Weight
              })
              .LastOrDefault(),
            r.ProviderSubmissions
              .OrderBy(ps => ps.Date)
              .Select(ps => ps.Date)
              .LastOrDefault(),
            r.ReferralSource))
          .ToListAsync();
      }

      static string GetResult(List<PreparedDischarge> prepareDischarges)
      {
        string result;
        int numProgComplete = prepareDischarges
          .Count(pd => pd.IsProgrammeOutcomeComplete);
        int numProgNotComplete = prepareDischarges
          .Count(pd => !pd.IsProgrammeOutcomeComplete);
        int numAwaitingDischarge = prepareDischarges
          .Count(pd => pd.IsAwaitingDischarge);
        int numComplete = prepareDischarges
          .Count(pd => !pd.IsAwaitingDischarge);
        int numOnHold = prepareDischarges
          .Count(pd => pd.IsDischargeOnHold);

        result = $"Found {prepareDischarges.Count} referrals to prepare.\r\n" +
          $"{numProgComplete} did complete the programme, whereas " +
          $"{numProgNotComplete} did not.\r\n" +
          $"{numAwaitingDischarge} are now awaiting discharge, " +
          $"{numOnHold} are now on hold, and " +
          $"{numComplete} are now complete.";
        return result;
      }
    }
  }
}

