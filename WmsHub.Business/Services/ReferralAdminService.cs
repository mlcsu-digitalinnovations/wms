using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using static WmsHub.Business.Enums.ReferralSource;
using static WmsHub.Business.Enums.ReferralStatus;

namespace WmsHub.Business.Services
{
  public class ReferralAdminService :
    ServiceBase<Referral>, IReferralAdminService
  {
    private readonly IReferralService _referralService;

    public ReferralAdminService(
      DatabaseContext context,
      IReferralService referralService)
      : base(context)
    {
      _referralService = referralService;
    }

    /// <summary>
    /// Changes a referral's date of birth
    /// </summary>
    /// <param name="ubrn">The UBRN to change to date of birth for</param>
    /// <param name="originalDateOfBirth">The current date of birth held for the
    /// UBRN's referral</param>
    /// <param name="updatedDateOfBirth">The date of birth to update the 
    /// referral with</param>
    /// <returns>A description of what was changed</returns>
    /// <exception cref="ArgumentNullOrWhiteSpaceException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="AgeOutOfRangeException"></exception>
    /// <exception cref="ReferralNotFoundException"></exception>
    public async Task<string> ChangeDateOfBirthAsync(
      string ubrn,
      DateTimeOffset originalDateOfBirth,
      DateTimeOffset updatedDateOfBirth)
    {
      if (string.IsNullOrWhiteSpace(ubrn))
      {
        throw new ArgumentNullOrWhiteSpaceException(nameof(ubrn));
      }

      if (updatedDateOfBirth == originalDateOfBirth)
      {
        throw new ArgumentException(
          $"Value must be different from {nameof(originalDateOfBirth)}.",
          nameof(updatedDateOfBirth));
      }

      if (updatedDateOfBirth.GetAge() < Constants.MIN_GP_REFERRAL_AGE ||
        updatedDateOfBirth.GetAge() > Constants.MAX_GP_REFERRAL_AGE)
      {
        throw new AgeOutOfRangeException($"The {nameof(updatedDateOfBirth)} " +
          $"must result in a service user's age between " +
          $"{Constants.MIN_GP_REFERRAL_AGE} and " +
          $"{Constants.MAX_GP_REFERRAL_AGE}.");
      }

      Referral referral = await _context.Referrals
        .Where(r => r.Ubrn == ubrn)
        .SingleOrDefaultAsync(r => r.IsActive);

      if (referral == null)
      {
        throw new ReferralNotFoundException(
          $"Unable to find a referral with a UBRN of {ubrn}.");
      }
      else if (referral.DateOfBirth != originalDateOfBirth)
      {
        throw new ReferralNotFoundException(
          $"Unable to find a referral with a UBRN of {ubrn} and a date of " +
          $"birth of {originalDateOfBirth:yyyy-MM-dd}.");
      }

      // The referral may need to be re-traiged if it has already been triaged
      if (referral.OfferedCompletionLevel != null)
      {
        // The referral can only be re-triaged if a provider has NOT been
        // selected
        if (referral.ProviderId == null)
        {
          _referralService.UpdateTriage(referral);
        }
      }

      referral.DateOfBirth = updatedDateOfBirth;
      UpdateModified(referral);

      await _context.SaveChangesAsync();

      return $"Date of birth for UBRN {ubrn} updated from " +
        $"{originalDateOfBirth} to {updatedDateOfBirth}.";
    }

    public async Task<string> ChangeMobileAsync(
      string ubrn,
      string originalMobile,
      string updatedMobile)
    {
      if (string.IsNullOrWhiteSpace(ubrn))
      {
        throw new ArgumentNullOrWhiteSpaceException(nameof(ubrn));
      }

      if (originalMobile == updatedMobile)
      {
        throw new ArgumentException(
          $"Value must be different from {nameof(originalMobile)}.",
          nameof(updatedMobile));
      }

      if (!updatedMobile.IsUkMobile())
      {
        throw new ArgumentOutOfRangeException(
          nameof(updatedMobile),
          $"Value is not a valid UK mobile number.");
      }

      Referral referral = await _context.Referrals
        .Where(r => r.Ubrn == ubrn)
        .SingleOrDefaultAsync(r => r.IsActive);

      if (referral == null)
      {
        throw new ReferralNotFoundException(
          $"Unable to find a referral with a UBRN of {ubrn}.");
      }
      else if (referral.Mobile != originalMobile)
      {
        throw new ReferralNotFoundException(
          $"Unable to find a referral with a UBRN of {ubrn} and a mobile " +
          $"of {originalMobile}.");
      }

      referral.Mobile = updatedMobile;
      referral.IsMobileValid = null;
      UpdateModified(referral);

      await _context.SaveChangesAsync();

      return $"Mobile for UBRN {ubrn} updated from {originalMobile} to " +
        $"{updatedMobile}.";
    }

    public async Task<string> DeleteCancelledGpReferralAsync(
      string ubrn,
      string reason)
    {
      if (string.IsNullOrWhiteSpace(reason))
      {
        throw new ArgumentException(
          $"'{nameof(reason)}' cannot be null or whitespace.", nameof(reason));
      }

      // find referral with the provided ubrn
      Referral referral = await _context.Referrals
        .Where(r => r.Ubrn == ubrn)
        .SingleOrDefaultAsync(r => r.IsActive);

      if (referral == null)
      {
        throw new ReferralNotFoundException(
          $"Unable to find a referral with a UBRN of '{ubrn}'.");
      }

      if (referral.Status != CancelledByEreferrals.ToString())
      {
        throw new ReferralInvalidStatusException(
          $"Expected referral with a UBRN of '{ubrn}' to have a status " +
          $"of '{CancelledByEreferrals}' but it has a status of " +
          $"'{referral.Status}'.");
      }

      if (referral.ReferralSource != GpReferral.ToString())
      {
        throw new ReferralInvalidReferralSourceException(
          $"Expected referral with a UBRN of '{ubrn}' to have a referral " +
          $"source of '{GpReferral}' but it has a referral source of " +
          $"'{referral.ReferralSource}'.");
      }

      referral.IsActive = false;
      referral.StatusReason = reason;
      UpdateModified(referral);

      await _context.SaveChangesAsync();

      return $"The referral with a UBRN of '{ubrn}' was deleted.";
    }

    public async Task<string> DeleteReferralAsync(
      Models.Referral referralToDelete)
    {
      if (referralToDelete is null)
      {
        throw new ArgumentNullException(nameof(referralToDelete));
      }

      // find referral with the provided ubrn
      Referral existingReferral = await _context.Referrals
        .Where(r => r.Id == referralToDelete.Id)
        .Where(r => r.Status == referralToDelete.Status)
        .Where(r => r.Ubrn == referralToDelete.Ubrn)
        .SingleOrDefaultAsync(r => r.IsActive);

      if (existingReferral == null)
      {
        throw new ReferralNotFoundException(
          $"Unable to find an active referral with an id of " +
          $"{referralToDelete.Id}, status of {referralToDelete.Status} " +
          $"and ubrn of {referralToDelete.Ubrn}.");
      }

      existingReferral.IsActive = false;
      existingReferral.StatusReason = referralToDelete.StatusReason;
      UpdateModified(existingReferral);

      await _context.SaveChangesAsync();

      return $"The referral with an id of {referralToDelete.Id} was deleted.";
    }

    public async Task<string> 
      FixNonGpReferralsWithStatusProviderCompletedAsync()
    {
      // get a list of referrals with a status of ProviderCompleted
      // that have a referral source that is not GP referral or null
      List<Referral> referrals = await _context.Referrals
        .Where(r => r.IsActive)
        .Where(r => r.Status == ProviderCompleted.ToString())
        .Where(r => (r.ReferralSource != GpReferral.ToString() &&
          r.ReferralSource != null))
        .ToListAsync();

      // update each referrals status to Complete
      referrals.ForEach(referral =>
      {
        referral.Status = Complete.ToString();
        UpdateModified(referral);
      });

      return $"{await _context.SaveChangesAsync()} non-GP referral(s) had " +
        $"their status updated from {ProviderCompleted} to {Complete}.";
    }

    public async Task<object> FixProviderAwaitingTraceAsync()
    {
      // get a list of referrals with a status of ProviderAwaitingTrace
      List<Referral> referrals = await _context.Referrals
        .Where(r => r.IsActive)
        .Where(r => r.Status == ProviderAwaitingTrace.ToString())
        .ToListAsync();

      int tracedProviderAwaitingStart = 0;
      int cancelledDuplicateTextMessage = 0;
      int untracedProviderAwaitingStart = 0;
      foreach (var referral in referrals)
      {
        // if NHS number is null 
        if (referral.NhsNumber == null)
        {
          // if referral has been previously traced, update status to 
          // ProviderAwaitingStart anyway so its not held in limbo if the trace
          // never succeeds
          if (referral.TraceCount >= 1)
          {
            untracedProviderAwaitingStart++;
            referral.Status = ProviderAwaitingStart.ToString();
            referral.StatusReason = "Status not updated to " +
              "ProviderAwaitingStart after first trace failure.";
            UpdateModified(referral);
          }
        }
        else
        {
          // does the traced NHS number match an existing referrals NHS number.
          // i.e. does it have a duplicate?
          List<Guid> duplicates = _context.Referrals
            .AsNoTracking()
            .Where(r => r.IsActive)
            .Where(r => r.NhsNumber == referral.NhsNumber)
            .Where(r => r.Id != referral.Id)
            .Select(r => r.Id)
            .ToList();

          UpdateModified(referral);

          if (duplicates.Any())
          {
            cancelledDuplicateTextMessage++;
            referral.Status = CancelledDuplicateTextMessage.ToString();
            referral.StatusReason = "Traced NHS number is a duplicate of " +
              $"existing referral id(s) {string.Join(", ", duplicates)}";
          }
          else
          {
            tracedProviderAwaitingStart++;
            referral.Status = ProviderAwaitingStart.ToString();
            referral.StatusReason = "Status incorrectly set to " +
              "ProviderAwaitingTrace after provider selection.";
          }
        }
      }

      await _context.SaveChangesAsync();

      return new
      {
        ProviderAwaitingTrace = referrals.Count,
        TracedProviderAwaitingStart = tracedProviderAwaitingStart,
        CancelledDuplicateTextMessage = cancelledDuplicateTextMessage,
        UntracedProviderAwaitingStart = untracedProviderAwaitingStart
      };

    }

    /// <summary>
    /// Resets an referral back to the provided status, resetting associated
    /// propeties
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ReferralNotFoundException"></exception>
    public async Task<Referral> ResetReferralAsync(
      Models.Referral referralToReset,
      ReferralStatus referralStatus)
    {
      if (referralToReset is null)
      {
        throw new ArgumentNullException(nameof(referralToReset));
      }

      if (referralStatus != New && referralStatus != RmcCall)
      {
        throw new NotSupportedException("Currently this process will only " +
          $"reset a referral's status to {New} or {RmcCall}");
      }

      Referral referral = await _context.Referrals
        .Include(r => r.ProviderSubmissions)
        .Where(r => r.Id == referralToReset.Id)
        .Where(r => r.Status == referralToReset.Status)
        .Where(r => r.Ubrn == referralToReset.Ubrn)
        .SingleOrDefaultAsync(r => r.IsActive);

      if (referral == null)
      {
        throw new ReferralNotFoundException(
          $"Unable to find an active referral with an id of " +
          $"{referralToReset.Id}, status of {referralToReset.Status} " +
          $"and ubrn of {referralToReset.Ubrn}.");
      }

      if (referralStatus == New)
      {
        if (referral.IsVulnerable == true)
        {
          referral.ResetToStatusRmcCall();
        }
        else
        {
          referral.ResetToStatusNew();
        }
      }
      else if (referralStatus == RmcCall)
      {
        referral.ResetToStatusRmcCall();
      }
      else
      {
        // shouldn't get here
        throw new ArgumentException("Currently this process will only reset" +
          $"a referral's status to {New} or {RmcCall}");
      }

      referral.StatusReason = referralToReset.StatusReason;
      UpdateModified(referral);

      await _context.SaveChangesAsync();

      referral.Audits = null;
      return referral;
    }
  }
}
