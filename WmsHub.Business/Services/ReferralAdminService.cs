using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Exceptions;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using static WmsHub.Business.Enums.ReferralSource;
using static WmsHub.Business.Enums.ReferralStatus;

namespace WmsHub.Business.Services;

public class ReferralAdminService : ServiceBase<Referral>,
  IReferralAdminService
{
  private readonly IReferralService _referralService;
  private readonly IMapper _mapper;

  public ReferralAdminService(
    DatabaseContext context,
    IReferralService referralService,
    IMapper mapper)
    : base(context)
  {
    _referralService = referralService;
    _mapper = mapper;
  }

  /// <inheritdoc/>
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

    if (updatedDateOfBirth.GetAge() < Constants.MIN_GP_REFERRAL_AGE
      || updatedDateOfBirth.GetAge() > Constants.MAX_GP_REFERRAL_AGE)
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
        _ = _referralService.UpdateTriage(referral);
      }
    }

    referral.DateOfBirth = updatedDateOfBirth;
    UpdateModified(referral);

    _ = await _context.SaveChangesAsync();

    return $"Date of birth for UBRN {ubrn} updated from " +
      $"{originalDateOfBirth} to {updatedDateOfBirth}.";
  }

  /// <inheritdoc/>
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

    _ = await _context.SaveChangesAsync();

    return $"Mobile for UBRN {ubrn} updated from {originalMobile} to " +
      $"{updatedMobile}.";
  }

  /// <inheritdoc/>
  public async Task<string> ChangeNhsNumberAsync(
    string ubrn,
    string originalNhsNumber,
    string updatedNhsNumber)
  {
    if (string.IsNullOrWhiteSpace(ubrn))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(ubrn));
    }

    if (originalNhsNumber == updatedNhsNumber)
    {
      throw new ArgumentException(
        $"Value must be different from {nameof(originalNhsNumber)}.",
        nameof(updatedNhsNumber));
    }

    if (!updatedNhsNumber.IsNhsNumber())
    {
      throw new ArgumentOutOfRangeException(
        nameof(updatedNhsNumber),
        $"Value is not a valid NHS number.");
    }

    await _referralService
      .CheckReferralCanBeCreatedWithNhsNumberAsync(updatedNhsNumber);

    Referral referral = await _context.Referrals
      .Where(r => r.Ubrn == ubrn)
      .SingleOrDefaultAsync(r => r.IsActive);

    if (referral == null)
    {
      throw new ReferralNotFoundException(
        $"Unable to find a referral with a UBRN of {ubrn}.");
    }
    else if (referral.NhsNumber != originalNhsNumber)
    {
      throw new ReferralNotFoundException(
        $"Unable to find a referral with a UBRN of {ubrn} and a NHS number " +
        $"of {originalNhsNumber}.");
    }

    referral.NhsNumber = updatedNhsNumber;
    UpdateModified(referral);

    _ = await _context.SaveChangesAsync();

    return $"NHS number for UBRN {ubrn} updated from {originalNhsNumber} to " +
      $"{updatedNhsNumber}.";
  }

  /// <inheritdoc/>
  public async Task<string> ChangeSexAsync(
    Guid id,
    string originalSex,
    string ubrn,
    string updatedSex)
  {
    ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty, nameof(id));
    ArgumentException.ThrowIfNullOrWhiteSpace(ubrn, nameof(ubrn));
    ArgumentException.ThrowIfNullOrWhiteSpace(updatedSex, nameof(updatedSex));

    if (originalSex?.Trim() == string.Empty)
    {
      throw new ArgumentException("Cannot be empty or white space.", nameof(originalSex));
    }

    if (originalSex == updatedSex)
    {
      throw new ArgumentException(
        $"'{nameof(originalSex)}' and '{nameof(updatedSex)}' cannot be the same.");
    }

    // TODO: Validation to be updated to only use Enum after full implementation of additional Sex values
    string[] validSexStrings =
    [
      Sex.Male.GetDescriptionAttributeValue(),
      Sex.Female.GetDescriptionAttributeValue(),
      Sex.NotSpecified.GetDescriptionAttributeValue(),
      Sex.NotKnown.GetDescriptionAttributeValue()
    ];

    if (!validSexStrings.Contains(updatedSex))
    {
      throw new ArgumentException( 
        $"'{updatedSex}' does not match permitted values for Sex: {string.Join(", ", validSexStrings)}.",
        nameof(updatedSex));
    }

    Referral referral = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Id == id)
      .Where(r => r.Sex == originalSex)
      .Where(r => r.Ubrn == ubrn)
      .SingleOrDefaultAsync()
      ?? throw new ReferralNotFoundException(id);

    referral.Sex = updatedSex;
    UpdateModified(referral);
    await _context.SaveChangesAsync();

    return $"Referral '{id}' has been updated to have a Sex of '{updatedSex}'.";
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
      .Include(r => r.ProviderSubmissions)
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

    if (referral.ReferralSource != ReferralSource.GpReferral.ToString())
    {
      throw new ReferralInvalidReferralSourceException(
        $"Expected referral with a UBRN of '{ubrn}' to have a referral " +
        $"source of '{ReferralSource.GpReferral}' but it has a referral " +
        $"source of '{referral.ReferralSource}'.");
    }

    referral.IsActive = false;
    referral.StatusReason = reason;
    UpdateModified(referral);

    foreach (ProviderSubmission providerSubmission in referral.ProviderSubmissions)
    {
      providerSubmission.IsActive = false;
      UpdateModified(providerSubmission);
    }

    _ = await _context.SaveChangesAsync();

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
      .Include(r => r.ProviderSubmissions)
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

    foreach (ProviderSubmission providerSubmission in existingReferral.ProviderSubmissions)
    {
      providerSubmission.IsActive = false;
      UpdateModified(providerSubmission);
    }

    _ = await _context.SaveChangesAsync();

    return $"The referral with an id of {referralToDelete.Id} was deleted.";
  }

  /// <inheritdoc/>
  public async Task<string>
    FixDeclinedOrRejectedReferralsWithMissingProgrammeOutcome()
  {
    IEnumerable<Referral> referrals =
      _context.Referrals
      .Where(r => r.ProgrammeOutcome == null)
      .Join(
        _context.ReferralsAudit
        .Where(
          ra => ra.Status == ProviderDeclinedByServiceUser.ToString()
          || ra.Status == ProviderDeclinedTextMessage.ToString()
          || ra.Status == ProviderRejected.ToString()
          || ra.Status == ProviderRejectedTextMessage.ToString()),
        r => r.Id,
        ra => ra.Id,
        (r, ra) => r);

    foreach (Referral referral in referrals)
    {
      referral.ProgrammeOutcome = ProgrammeOutcome.DidNotCommence.ToString();
      UpdateModified(referral);
    }

    return $"{await _context.SaveChangesAsync()} referral(s) had their " +
      $"ProgrammeOutcome updated to {ProgrammeOutcome.DidNotCommence}";
  }

  public async Task<object> FixGPReferralsWithStatusLetterOrLetterSent()
  {
    // get a list of GP referrals with a status of Letter or LetterSent
    List<Referral> referrals = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.ReferralSource == ReferralSource.GpReferral.ToString())
      .Where(
        r => r.Status == Letter.ToString()
        || r.Status == LetterSent.ToString()
      )
      .ToListAsync();

    int letter = 0;
    int letterSent = 0;

    foreach (Referral referral in referrals)
    {
      if (referral.Status == Letter.ToString())
      {
        letter++;
      }
      else if (referral.Status == LetterSent.ToString())
      {
        letterSent++;
      }
      else
      {
        throw new ReferralInvalidStatusException(
          $"Currently this process will only fix referrals with a status " +
          $"of {Letter} or {LetterSent}");
      }

      referral.ProgrammeOutcome = ProgrammeOutcome.DidNotCommence.ToString();
      referral.Status = RejectedToEreferrals.ToString();
      UpdateModified(referral);
    }

    _ = await _context.SaveChangesAsync();

    return new
    {
      LetterOrLetterSent = referrals.Count,
      LetterUpdatedToRejectedToEreferrals = letter,
      LetterSentUpdatedToRejectedToEreferrals = letterSent
    };
  }

  public async Task<string> FixMSKReferralsWithStatusRejectedToEreferrals()
  {
    List<Referral> referrals = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.ReferralSource == Msk.ToString())
      .Where(r => r.Status == RejectedToEreferrals.ToString())
      .ToListAsync();

    foreach (Referral referral in referrals)
    {
      referral.Status = CancelledByEreferrals.ToString();
      UpdateModified(referral);
    }

    return $"{await _context.SaveChangesAsync()} MSK referral(s) had " +
      $"their status updated from {RejectedToEreferrals} to " +
      $"{CancelledByEreferrals}.";
  }

  public async Task<string>
    FixNonGpReferralsWithStatusProviderCompletedAsync()
  {
    // get a list of referrals with a status of ProviderCompleted
    // that have a referral source that is not GP referral or null
    List<Referral> referrals = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Status == ProviderCompleted.ToString())
      .Where(r => r.ReferralSource != ReferralSource.GpReferral.ToString()
        && r.ReferralSource != null)
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

  /// <inheritdoc/>
  public async Task<string> FixNumbersAsync(string ubrn)
  {
    if (string.IsNullOrWhiteSpace(ubrn))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(ubrn));
    }

    Referral referral = await _context.Referrals
      .Where(r => r.Ubrn == ubrn)
      .SingleOrDefaultAsync(r => r.IsActive);

    if (referral == null)
    {
      throw new ReferralNotFoundException(
        $"Unable to find a referral with a UBRN of {ubrn}.");
    }

    string originalMobile = referral.Mobile;
    string originalTelephone = referral.Telephone;
    bool? originalIsMobileValid = referral.IsMobileValid;
    bool? originalIsTelephoneValid = referral.IsTelephoneValid;

    PhoneHelper.FixPhoneNumberFields(referral);

    if (referral.Mobile != originalMobile
      || referral.Telephone != originalTelephone
      || referral.IsMobileValid != originalIsMobileValid
      || referral.IsTelephoneValid != originalIsTelephoneValid)
    {
      UpdateModified(referral);
      _ = await _context.SaveChangesAsync();
    }

    string result = referral.Mobile == originalMobile
      ? $"Mobile for UBRN {ubrn} was not updated from " +
        $"{originalMobile}."
      : $"Mobile for UBRN {ubrn} updated from {originalMobile} to " +
        $"{referral.Mobile}.";

    if (referral.Telephone == originalTelephone)
    {
      result += $" Telephone for UBRN {ubrn} was not updated from " +
        $"{originalTelephone}.";
    }
    else
    {
      result += $" Telephone for UBRN {ubrn} updated from " +
        $"{originalTelephone} to {referral.Telephone}.";
    }

    return result;
  }

  public async Task<object> FixPharmacyReferralsWithInvalidStatus()
  {
    List<Referral> referrals = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.ReferralSource == ReferralSource.Pharmacy.ToString())
      .Where(r => r.Status == Letter.ToString()
        || r.Status == LetterSent.ToString()
        || r.Status == RejectedToEreferrals.ToString()
      )
      .ToListAsync();

    int letter = 0;
    int letterSent = 0;
    int rejectedToEreferrals = 0;

    foreach (Referral referral in referrals)
    {
      if (referral.Status == Letter.ToString())
      {
        letter++;
      }
      else if (referral.Status == LetterSent.ToString())
      {
        letterSent++;
      }
      else if (referral.Status == RejectedToEreferrals.ToString())
      {
        rejectedToEreferrals++;
      }
      else
      {
        throw new ReferralInvalidStatusException(
          $"Currently this process will only fix referrals with a status " +
          $"of {Letter}, {LetterSent} or {RejectedToEreferrals}");
      }

      referral.ProgrammeOutcome = ProgrammeOutcome.DidNotCommence.ToString();
      referral.Status = CancelledByEreferrals.ToString();
      UpdateModified(referral);
    }

    _ = await _context.SaveChangesAsync();

    return new
    {
      InvalidStatus = referrals.Count,
      LetterUpdatedToCancelledByEreferrals = letter,
      LetterSentUpdatedToCancelledByEreferrals = letterSent,
      RejectedToEreferralsUpdatedToCancelledByEreferrals =
        rejectedToEreferrals
    };
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
    foreach (Referral referral in referrals)
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

    _ = await _context.SaveChangesAsync();

    return new
    {
      ProviderAwaitingTrace = referrals.Count,
      TracedProviderAwaitingStart = tracedProviderAwaitingStart,
      CancelledDuplicateTextMessage = cancelledDuplicateTextMessage,
      UntracedProviderAwaitingStart = untracedProviderAwaitingStart
    };
  }

  public async Task<object> FixReferralProviderUbrnAsync()
  {
    int numIgnores = 0;

    // swap the Ubrn field with the provider ubrn where the Ubrn field
    // contains a GP prefix
    List<Referral> referrals = await _context.Referrals
      .Where(r => r.ProviderUbrn != null)
      .ToListAsync();

    foreach (Referral referral in referrals)
    {

      if (referral.Ubrn.StartsWith("GP")
        && referral.ProviderUbrn.StartsWith("00"))
      {
        string updatedProviderUbrn = referral.Ubrn;
        string updatedUbrn = referral.ProviderUbrn;

        referral.Ubrn = updatedUbrn;
        referral.ProviderUbrn = updatedProviderUbrn;
      }
      else
      {
        numIgnores++;
      }
    }

    int numUpdates = await _context.SaveChangesAsync();

    return $"Updated {numUpdates} referrals. Ignored {numIgnores}";
  }

  /// <inheritdoc/>
  public async Task<List<string>> FixReferralsWithInvalidStatuses()
  {
    List<string> outcomes = new();
    List<Referral> referrals = await _context.Referrals
      .Where(x => x.IsActive)
      .Where(x => x.ReferralSource == ReferralSource.ElectiveCare.ToString()
        || x.ReferralSource == ReferralSource.GeneralReferral.ToString()
        || x.ReferralSource == ReferralSource.Msk.ToString()
        || x.ReferralSource == ReferralSource.Pharmacy.ToString()
        || x.ReferralSource == ReferralSource.SelfReferral.ToString())
      .Where(x => x.Status == CancelledByEreferrals.ToString()
        || x.Status == CancelledDueToNonContact.ToString()
        || x.Status == CancelledDuplicate.ToString()
        || x.Status == FailedToContact.ToString()
        || x.Status == Letter.ToString()
        || x.Status == LetterSent.ToString()
        || x.Status == RejectedToEreferrals.ToString())
      .ToListAsync();

    referrals.AddRange(await _context.Referrals
      .Where(x => x.IsActive)
      .Where(x => x.ReferralSource == ReferralSource.GpReferral.ToString())
      .Where(x => x.Status == CancelledDueToNonContact.ToString()
        || x.Status == CancelledDuplicate.ToString()
        || x.Status == DischargeOnHold.ToString()
        || x.Status == Letter.ToString()
        || x.Status == LetterSent.ToString())
      .ToArrayAsync());

    referrals.AddRange(await _context.Referrals
      .Where(x => x.IsActive)
      .Where(x => x.ReferralSource != ReferralSource.GpReferral.ToString())
      .Where(x => x.Status == ReferralStatus.Exception.ToString())
      .Where(x => x.Email == Constants.DO_NOT_CONTACT_EMAIL)
      .ToArrayAsync());

    foreach (Referral referral in referrals)
    {
      referral.StatusReason = $"Fixed invalid status {referral.Status}";
      referral.Status = Complete.ToString();
      UpdateModified(referral);

      outcomes.Add($"{referral.Id} {referral.StatusReason}");
    }

    _ = await _context.SaveChangesAsync();

    return outcomes;
  }

  /// <inheritdoc/>
  public async Task<List<string>>
    FixReferralsWithMissingDateStartedProgramme()
  {
    List<string> output = new();

    List<Referral> referrals = await _context.Referrals
      .Include(r => r.ProviderSubmissions)
      .Where(r => r.IsActive)
      .Where(r => r.ProviderId != null)
      .Where(r => r.DateStartedProgramme == null)
      .Where(r => r.ProviderSubmissions.Any())
      .ToListAsync();

    foreach (Referral referral in referrals)
    {
      ProviderSubmission firstSubmission
        = referral.ProviderSubmissions.OrderBy(s => s.Date).First();

      if (firstSubmission.ProviderId == referral.ProviderId)
      {
        referral.DateStartedProgramme
          = firstSubmission.Date;

        output.Add($"DateStartedProgramme for Referral {referral.Id} updated" +
        $" to {referral.DateStartedProgramme}.");
      }
      else
      {
        output.Add($"Mismatch between ProviderId for Referral {referral.Id} " +
          $"and ProviderSubmission {firstSubmission.Id}. No update made.");
      }
    }

    if (output.Count == 0)
    {
      output.Add("No referrals required updates.");
    }

    return output;
  }

  /// <inheritdoc/>
  public async Task<List<string>> FixReferralsWithNullProviderUbrn()
  {
    List<string> outcomes = new();

    // Get a list of referrals that have a null provider ubrn.
    List<Referral> referrals = await _context.Referrals
      .Where(r => r.ProviderUbrn == null)
      .ToListAsync();

    foreach (Referral referral in referrals)
    {
      try
      {
        if (Enum.TryParse(
          typeof(ReferralSource),
          referral.ReferralSource,
          out object referralSource))
        {
          switch (referralSource)
          {
            case ReferralSource.GeneralReferral:
              await _referralService
                .UpdateGeneralReferralUbrnAsync(referral.Id);
              break;
            case ReferralSource.GpReferral:
              await _referralService.UpdateGpReferralUbrnAsync(referral.Id);
              break;
            case Msk:
              await _referralService.UpdateMskReferralUbrnAsync(referral.Id);
              break;
            case ReferralSource.Pharmacy:
              await _referralService
                .UpdatePharmacyReferralUbrnAsync(referral.Id);
              break;
            case ReferralSource.SelfReferral:
              await _referralService.UpdateSelfReferralUbrnAsync(referral.Id);
              break;
            default:
              outcomes.Add($"[ERR] Found unsupported referral source " +
                $"{referral.ReferralSource} for referral id {referral.Id}.");
              continue;
          }

          outcomes.Add($"[INF] Updated referral {referral.Id}");
        }
        else
        {
          outcomes.Add($"[ERR] Found unknown referral source " +
            $"{referral.ReferralSource} for referral id {referral.Id}.");
        }
      }
      catch (Exception ex)
      {
        outcomes.Add($"[ERR] {ex.Message}");
      }
    }

    if (outcomes.Count == 0)
    {
      outcomes.Add("Nothing to do.");
    }

    return outcomes;
  }

  public async Task<object> FixSelfReferralsWithInvalidStatus()
  {
    // get a list of GP referrals with a status of Letter or LetterSent
    List<Referral> referrals = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.ReferralSource == ReferralSource.SelfReferral.ToString())
      .Where(r => r.Status == Letter.ToString()
        || r.Status == LetterSent.ToString()
        || r.Status == RejectedToEreferrals.ToString()
        || r.Status == FailedToContact.ToString()
      )
      .ToListAsync();

    int letter = 0;
    int letterSent = 0;
    int rejectedToEreferrals = 0;
    int failedToContact = 0;

    foreach (Referral referral in referrals)
    {

      if (referral.Status == Letter.ToString()
        || referral.Status == LetterSent.ToString())
      {
        if (referral.Status == Letter.ToString())
        {
          letter++;
        }
        else if (referral.Status == LetterSent.ToString())
        {
          letterSent++;
        }

        referral.ProgrammeOutcome = ProgrammeOutcome
          .DidNotCommence
          .ToString();
        referral.Status = CancelledByEreferrals.ToString();
      }
      else if (referral.Status == RejectedToEreferrals.ToString())
      {
        rejectedToEreferrals++;
        referral.Status = CancelledByEreferrals.ToString();
      }
      else if (referral.Status == FailedToContact.ToString())
      {
        failedToContact++;
        referral.Status = CancelledDueToNonContact.ToString();
      }
      else
      {
        throw new ReferralInvalidStatusException(
          $"Currently this process will only fix referrals with a status " +
          $"of {Letter}, {LetterSent}, {RejectedToEreferrals} or " +
          FailedToContact.ToString());
      }

      UpdateModified(referral);
    }

    _ = await _context.SaveChangesAsync();

    return new
    {
      InvalidStatus = referrals.Count,
      LetterUpdatedToCancelledByEreferrals = letter,
      LetterSentUpdatedToCancelledByEreferrals = letterSent,
      RejectedToEreferralsUpdatedToCancelledByEreferrals =
        rejectedToEreferrals,
      FailedToContactUpdatedToCancelledDueToNonContact = failedToContact,
    };
  }

  /// <inheritdoc/>
  public async Task<Models.Referral> ResetReferralAsync(
    Models.Referral referralToReset,
    ReferralStatus referralStatus)
  {
    if (referralToReset is null)
    {
      throw new ArgumentNullException(nameof(referralToReset));
    }

    if (referralStatus is not New and not RmcCall)
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

    Models.Referral referralModel = _mapper.Map<Models.Referral>(referral);

    return referralModel;
  }

  /// <inheritdoc/>
  public async Task<List<Guid>>
    SendDischargeLettersForCompleteMskReferrals(
      DateTimeOffset fromDate,
      DateTimeOffset toDate)
  {
    if (fromDate > toDate)
    {
      throw new DateRangeNotValidException("FromDate must be earlier than " +
        "ToDate.");
    }

    if (fromDate > DateTimeOffset.UtcNow || toDate > DateTimeOffset.UtcNow)
    {
      throw new DateRangeNotValidException("Dates cannot be in the future.");
    }

    // Get all complete MSK referrals within date bounds that have not already
    // been sent for discharge or failed discharge.

    IQueryable<MskOrganisation> mskOrgs = _context.MskOrganisations
      .Where(o => o.IsActive)
      .Where(o => o.SendDischargeLetters);

    IQueryable<Referral> referrals = _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Status == Complete.ToString())
      .Where(r => r.ReferralSource == Msk.ToString())
      .Where(r => r.ConsentForReferrerUpdatedWithOutcome.HasValue
        && r.ConsentForReferrerUpdatedWithOutcome.Value == true)
      .Where(r => r.DateOfReferral >= fromDate)
      .Where(r => r.DateOfReferral <= toDate);

    IQueryable<Guid> sentDischargeReferrals = _context.ReferralsAudit
      .Where(
        ra => ra.Status == SentForDischarge.ToString()
        || ra.Status == UnableToDischarge.ToString())
      .Select(ra => ra.Id);

    IQueryable<Guid> query = mskOrgs
      .Join(
        referrals,
        o => o.OdsCode,
        r => r.ReferringOrganisationOdsCode,
        (o, r) => r.Id)
      .Except(sentDischargeReferrals);

    List<Guid> referralIdsToProcess = await query.ToListAsync();

    string[] outcomesRequiringMessage = GpDocumentProxyHelper.ProgrammeOutcomesRequiringMessage();

    List<GpDocumentProxyReferralDischarge> discharges = new();

    foreach (Guid id in referralIdsToProcess)
    {
      Referral referral = await _context.Referrals
        .Include("Provider")
        .Where(r => r.Id == id)
        .SingleOrDefaultAsync();

      if (referral == null)
      {
        continue;
      }

      discharges.Add(new());
      discharges.Add(new());

      referral.Status = SentForDischarge.ToString();
      UpdateModified(referral);
      await _context.SaveChangesAsync();
    }

    if (!discharges.Any())
    {
      return new List<Guid>();
    }

    List<Guid> sentDischarges =
      await _referralService.PostDischarges(discharges);

    // Reset status to Complete - note, status will not now be updated by calls
    // to Referral API UpdateDischarges so final outcome of discharge letter
    // will not be recorded.
    foreach (Referral referral in referrals)
    {
      referral.Status = Complete.ToString();
      UpdateModified(referral);
    }

    await _context.SaveChangesAsync();

    return sentDischarges;
  }

  ///<inheritdoc/>
  public async Task<Models.Referral> SetIsErsClosedToFalse(Guid id, string ubrn)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentOutOfRangeException(nameof(id), "Cannot be empty.");
    }

    if (string.IsNullOrWhiteSpace(ubrn))
    {
      throw new ArgumentNullOrWhiteSpaceException(nameof(ubrn));
    }

    Referral referral = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.ReferralSource == ReferralSource.GpReferral.ToString())
      .Where(r => r.Status == RejectedToEreferrals.ToString())
      .Where(r => r.IsErsClosed == true)
      .Where(r => r.Id == id)
      .Where(r => r.Ubrn == ubrn)
      .SingleOrDefaultAsync()
      ?? throw new ReferralNotFoundException(
        $"Unable to find a GP referral with an Id of {id} and a Ubrn of {ubrn} that has a Status " +
        $"of {nameof(RejectedToEreferrals)} and IsErsClosed = true.");

    referral.IsErsClosed = false;
    UpdateModified(referral);
    await _context.SaveChangesAsync();

    Models.Referral referralModel = _mapper.Map<Models.Referral>(referral);

    return referralModel;
  }

  /// <inheritdoc/>
  public async Task<List<Guid>> SetMismatchedEthnicityToNull(Guid[] ids)
  {
    ArgumentNullException.ThrowIfNull(ids);
    ArgumentOutOfRangeException.ThrowIfZero(ids.Length);

    List<Referral> referralsToUpdate = await _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.Ethnicity != null)
      .Where(r => ids.Contains(r.Id))
      .Join(_context.Ethnicities.Where(e => e.IsActive),
        r => r.ServiceUserEthnicity,
        e => e.DisplayName,
        (r, e) => new { Referral = r, e.TriageName })
      .Where(x => x.Referral.Ethnicity != x.TriageName)
      .Select(x => x.Referral)
      .ToListAsync();

    foreach (Referral referral in referralsToUpdate)
    {
      referral.Ethnicity = null;
    }

    await _context.SaveChangesAsync();

    return referralsToUpdate.Select(r => r.Id).ToList();
  }

  /// <inheritdoc/>
  public async Task<List<Guid>> UpdateMskReferringOrganisationOdsCode(
    string currentOdsCode,
    string newOdsCode)
  {
    foreach (string odsCode in new string[] {currentOdsCode, newOdsCode})
    {
      if (string.IsNullOrWhiteSpace(odsCode))
      {
        throw new ArgumentException("Both ODS codes must be provided.");
      }

      bool mskOrganisationExists = _context
        .MskOrganisations
        .Where(m => m.IsActive)
        .Where(m => m.OdsCode == odsCode)
        .Any();

      if (!mskOrganisationExists)
      {
        throw new MskOrganisationNotFoundException("No MskOrganisation found" +
          $" with OdsCode {odsCode}. No referrals were updated.");
      }
    }

    List<Referral> referrals = await _context
      .Referrals
      .Where(r => r.IsActive)
      .Where(r => r.ReferringOrganisationOdsCode == currentOdsCode)
      .ToListAsync();

    foreach (Referral referral in referrals)
    {
      referral.ReferringOrganisationOdsCode = newOdsCode;
      UpdateModified(referral);
    }

    await _context.SaveChangesAsync();

    return referrals.Select(r => r.Id).ToList();
  }
}
