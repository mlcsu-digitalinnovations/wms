using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Models.Discharge;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Models.ReferralService;
using static WmsHub.Business.Enums.ReferralStatus;

namespace WmsHub.Business.Services;

public class ReferralDischargeService : ServiceBase<Referral>, IReferralDischargeService
{
  private static bool IsPrepareDischargesInProgress = false;
  private readonly ProviderOptions _providerOptions;
  private readonly ReferralTimelineOptions _referralTimelineOptions;

  public ReferralDischargeService(
    DatabaseContext context,
    IOptions<ProviderOptions> providerOptions,
    IOptions<ReferralTimelineOptions> referralTimelineOptions)
    : base(context)
  {
    _providerOptions = providerOptions.Value;
    _referralTimelineOptions = referralTimelineOptions.Value;
    PreparedDischarge.SetOptions(_providerOptions, _referralTimelineOptions);
  }

  public async Task<string> PrepareDischarges()
  {
    if (IsPrepareDischargesInProgress)
    {
      return "PrepareDischarges is already in progress. Please wait for it to complete.";
    }

    string result = null;
    try
    {
      IsPrepareDischargesInProgress = true;

      List<PreparedDischarge> preparedDischarges = await GetPreparedDischargesAsync();

      foreach (PreparedDischarge preparedDischarge in preparedDischarges)
      {
        Referral referral = await _context.Referrals.FindAsync(preparedDischarge.Id);

        referral.DateCompletedProgramme = preparedDischarge.DateOfLastEngagement;
        referral.FirstRecordedWeight = preparedDischarge.FirstRecordedWeight;
        referral.FirstRecordedWeightDate = preparedDischarge.FirstRecordedWeightDate;
        referral.LastRecordedWeight = preparedDischarge.LastRecordedWeight;
        referral.LastRecordedWeightDate = preparedDischarge.LastRecordedWeightDate;
        referral.ProgrammeOutcome = preparedDischarge.ProgrammeOutcome;
        referral.Status = preparedDischarge.Status;
        referral.StatusReason = preparedDischarge.StatusReason;

        UpdateModified(referral);
      }

      await _context.SaveChangesAsync();

      // Update the referrals that are unable to discharge to complete.
      foreach (PreparedDischarge preparedDischarge in preparedDischarges
        .Where(x => x.IsUnableToDischarge))
      {
        Referral referral = await _context.Referrals.FindAsync(preparedDischarge.Id);
        referral.Status = Complete.ToString();
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
  }

  public virtual IQueryable<Referral> GetReferralsForDischargeBySourceQuery()
  {
    DateTimeOffset dischargeDate = DateTimeOffset.Now
      .AddDays(-_providerOptions.DischargeAfterDays);

    IQueryable<Referral> programmeStartedQuery = _context.Referrals
      .AsNoTracking()
      .Where(r => r.IsActive)
      .Where(r => r.DateStartedProgramme != null)
      .Where(r => r.DateStartedProgramme < dischargeDate)
      .Where(r => r.Status == ProviderCompleted.ToString()
        || r.Status == ProviderContactedServiceUser.ToString()
        || r.Status == ProviderDeclinedByServiceUser.ToString()
        || r.Status == ProviderRejected.ToString()
        || r.Status == ProviderStarted.ToString()
        || r.Status == ProviderTerminated.ToString());

    IQueryable<Referral> programmeNotStartedQuery = _context.Referrals
      .AsNoTracking()
      .Where(r => r.IsActive)
      .Where(r => r.DateStartedProgramme == null)
      .Where(r => r.Status == ProviderTerminated.ToString());

    return programmeStartedQuery.Concat(programmeNotStartedQuery);
  }

  public virtual async Task<List<PreparedDischarge>>
    GetPreparedDischargesAsync()
  {
    IQueryable<Referral> referralsQuery = GetReferralsForDischargeBySourceQuery();

    IQueryable<Guid> referralIdsToSendDischargeLetters = referralsQuery.Select(r => r.Id);

    List<PreparedDischarge> preparedDischarges =
      await GetPreparedDischargesFromReferralIds(referralIdsToSendDischargeLetters);

    return preparedDischarges;
  }

  private async Task<List<PreparedDischarge>>
    GetPreparedDischargesFromReferralIds(IQueryable<Guid> referralIds)
  {
    if (referralIds == null || !referralIds.Any())
    {
      return [];
    }

    return await _context.Referrals
      .Include(r => r.ProviderSubmissions)
      .Where(r => referralIds.Contains(r.Id))
      .Select(r => new PreparedDischarge(
        r.Id,
        r.DateOfProviderSelection.Value,
        r.DateStartedProgramme,
        r.DateCompletedProgramme,
        r.NhsNumber,
        r.ProviderSubmissions
          .Select(ps => new ProviderSubmission
          {
            Date = ps.Date,
            ModifiedAt = ps.ModifiedAt,
            Weight = ps.Weight
          })
          .ToList(),
        r.ReferringGpPracticeNumber))
      .ToListAsync();
  }

  private static string GetResult(List<PreparedDischarge> prepareDischarges)
  {
    string result;
    int numProgComplete = prepareDischarges.Count(pd => pd.IsProgrammeOutcomeComplete);
    int numProgNotComplete = prepareDischarges.Count(pd => !pd.IsProgrammeOutcomeComplete);
    int numAwaitingDischarge = prepareDischarges.Count(pd => pd.IsAwaitingDischarge);
    int numUnableToDischarge = prepareDischarges.Count(pd => pd.IsUnableToDischarge);
    int numComplete = prepareDischarges.Count(pd => !pd.IsAwaitingDischarge);
    int numOnHold = prepareDischarges.Count(pd => pd.IsDischargeOnHold);

    result = $"Found {prepareDischarges.Count} referrals to prepare.\r\n" +
      $"{numProgComplete} did complete the programme, whereas " +
      $"{numProgNotComplete} did not.\r\n" +
      $"{numAwaitingDischarge} are now awaiting discharge, " +
      $"{numUnableToDischarge} are unable to be discharged, " +
      $"{numOnHold} are now on hold, and " +
      $"{numComplete} are now complete.";
    return result;
  }
}

