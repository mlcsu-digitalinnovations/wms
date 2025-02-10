using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Extensions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.BusinessIntelligence;
using WmsHub.Business.Models.ElectiveCareReferral;
using WmsHub.Business.Models.Tracing;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using Provider = WmsHub.Business.Entities.Provider;
using ProviderSubmission = WmsHub.Business.Entities.ProviderSubmission;
using Referral = WmsHub.Business.Entities.Referral;

namespace WmsHub.Business.Services;

public class BusinessIntelligenceService : ServiceBase<Referral>, IBusinessIntelligenceService
{
  private readonly IMapper _mapper;
  private readonly ILogger _logger;
  private readonly BusinessIntelligenceOptions _options;

  public BusinessIntelligenceService(
    DatabaseContext context,
    IMapper mapper,
    IOptions<BusinessIntelligenceOptions> options,
    ILogger log)
    : base(context)
  {
    _mapper = mapper;
    _logger = log;
    _options = options == null
      ? throw new ArgumentNullException(
        $"{nameof(IOptions<BusinessIntelligenceOptions>)} is null.")
      : options.Value ?? throw new ArgumentNullException(
          $"{nameof(BusinessIntelligenceOptions)} is null.");
  }

  public async Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferrals(
    DateTimeOffset? fromDate = null,
    DateTimeOffset? toDate = null)
  {
    fromDate ??= new DateTimeOffset();
    toDate ??= DateTimeOffset.Now;

    return await GetAnonymisedReferralsForPredicateAsync(
      (Referral r) =>
        r.DateOfReferral >= fromDate && r.DateOfReferral <= toDate);
  }

  public async Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferralsByModifiedAt(
    DateTimeOffset lastModifiedDate)
  {
    return await GetAnonymisedReferralsForPredicateAsync(
      (Referral r) => r.ModifiedAt >= lastModifiedDate);
  }

  /// <inheritdoc/>
  public async Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferralsByProviderReason(
    DateTimeOffset? fromDate,
    DateTimeOffset? toDate,
    ReferralStatus status)
  {
    fromDate ??= new DateTimeOffset();
    toDate ??= DateTimeOffset.Now;

    ReferralStatus expectedStatusFlag =
      ReferralStatus.ProviderDeclinedByServiceUser |
      ReferralStatus.ProviderDeclinedTextMessage |
      ReferralStatus.ProviderTerminated |
      ReferralStatus.ProviderTerminatedTextMessage |
      ReferralStatus.ProviderRejected |
      ReferralStatus.ProviderRejectedTextMessage;

    if (!expectedStatusFlag.HasFlag(status))
    {
      throw new ReferralInvalidStatusException(
        $"{status} is not accepted for this method.");
    }

    try
    {
      string[] lookup = status.ToString().Replace(" ", "").Split(',');
      List<Referral> entities = await _context
        .Referrals
        .Include(r => r.ProviderSubmissions.Where(ps => ps.IsActive))
        .Include(r => r.Provider)
        .Where(r => r.IsActive)
        .Where(r => r.DateOfReferral >= fromDate
          && r.DateOfReferral <= toDate)
        .Join(
          _context.ReferralsAudit.Where(a => lookup.Contains(a.Status)),
          src => src.Id,
          audit => audit.Id,
          (src, audit) => new { Audit = audit, Referral = src })
        .AsNoTracking()
        .Where(r => r.Audit != null)
        .Select(r => new Referral
        {
          DateOfBirth = r.Referral.DateOfBirth,
          CalculatedBmiAtRegistration = r.Referral.CalculatedBmiAtRegistration,
          ModifiedAt = r.Referral.ModifiedAt,
          ConsentForFutureContactForEvaluation =
            r.Referral.ConsentForFutureContactForEvaluation,
          DateCompletedProgramme = r.Referral.DateCompletedProgramme,
          DateOfBmiAtRegistration = r.Referral.DateOfBmiAtRegistration,
          DateOfProviderContactedServiceUser =
            r.Referral.DateOfProviderContactedServiceUser,
          DateOfProviderSelection = r.Referral.DateOfProviderSelection,
          DateOfReferral = r.Referral.DateOfReferral,
          DateStartedProgramme = r.Referral.DateStartedProgramme,
          DatePlacedOnWaitingList = r.Referral.DatePlacedOnWaitingList,
          DateToDelayUntil = r.Referral.DateToDelayUntil,
          Deprivation = r.Referral.Deprivation,
          Ethnicity = r.Referral.Ethnicity,
          ServiceUserEthnicityGroup = r.Referral.ServiceUserEthnicityGroup,
          ServiceUserEthnicity = r.Referral.ServiceUserEthnicity,
          WeightKg = r.Referral.WeightKg,
          HasALearningDisability = r.Referral.HasALearningDisability,
          HasAPhysicalDisability = r.Referral.HasAPhysicalDisability,
          HasDiabetesType1 = r.Referral.HasDiabetesType1,
          HasDiabetesType2 = r.Referral.HasDiabetesType2,
          HasHypertension = r.Referral.HasHypertension,
          HasRegisteredSeriousMentalIllness =
            r.Referral.HasRegisteredSeriousMentalIllness,
          HeightCm = r.Referral.HeightCm,
          Id = r.Referral.Id,
          IsActive = r.Referral.IsActive,
          IsVulnerable = r.Referral.IsVulnerable,
          MethodOfContact = r.Referral.MethodOfContact,
          ModifiedByUserId = r.Referral.ModifiedByUserId,
          NumberOfContacts = r.Referral.NumberOfContacts,
          OpcsCodes = r.Referral.OpcsCodes,
          ProgrammeOutcome = r.Referral.ProgrammeOutcome,
          Provider = new Provider
          {
            Name = r.Referral.Provider.Name
          },
          ProviderSubmissions = r.Referral.ProviderSubmissions
          .GroupBy(ps => new
          {
            ps.Coaching,
            ps.Date,
            ps.Measure,
            ps.ReferralId,
            ps.Weight,
            ps.Id
          })
          .Select(g => new ProviderSubmission
          {
            Coaching = g.Key.Coaching,
            Date = g.Key.Date,
            Id = g.Key.Id,
            Measure = g.Key.Measure,
            ModifiedAt = g.Max(ps => ps.ModifiedAt),
            ReferralId = g.Key.ReferralId,
            Weight = g.Key.Weight
          })
          .ToList(),
          ReferralLetterDate = r.Referral.ReferralLetterDate,
          ReferralSource = r.Referral.ReferralSource,
          ReferringGpPracticeNumber = r.Referral.ReferringGpPracticeNumber,
          Sex = r.Referral.Sex,
          Status = r.Referral.Status,
          StatusReason = r.Referral.StatusReason,
          TriagedCompletionLevel = r.Referral.TriagedCompletionLevel,
          Ubrn = r.Referral.Ubrn,
          VulnerableDescription = r.Referral.VulnerableDescription,
          StaffRole = r.Referral.StaffRole,
          ConsentForGpAndNhsNumberLookup = r.Referral.ConsentForGpAndNhsNumberLookup,
          ConsentForReferrerUpdatedWithOutcome =
            r.Referral.ConsentForReferrerUpdatedWithOutcome,
          ReferringOrganisationEmail = r.Referral.ReferringOrganisationEmail,
          ReferringOrganisationOdsCode = r.Referral.ReferringOrganisationOdsCode,
          HasActiveEatingDisorder = r.Referral.HasActiveEatingDisorder,
          HasArthritisOfKnee = r.Referral.HasArthritisOfKnee,
          HasArthritisOfHip = r.Referral.HasArthritisOfHip,
          IsPregnant = r.Referral.IsPregnant,
          HasHadBariatricSurgery = r.Referral.HasHadBariatricSurgery,
          SourceSystem = r.Referral.SourceSystem,
          DocumentVersion = r.Referral.DocumentVersion,
          ServiceId = r.Referral.ServiceId,
          ProviderUbrn = r.Referral.ProviderUbrn
        })
        .OrderBy(o => o.DateOfReferral)
        .ToListAsync();

      foreach (Referral entity in entities)
      {
        entity.InjectionRemover();
      }

      IEnumerable<AnonymisedReferral> anonymisedReferralList = entities
        .Select(r => new AnonymisedReferral
        {
          Age = r.DateOfBirth?.GetAge(),
          CalculatedBmiAtRegistration = r.CalculatedBmiAtRegistration,
          ModifiedAt = r.ModifiedAt,
          ConsentForFutureContactForEvaluation =
            r.ConsentForFutureContactForEvaluation,
          DateCompletedProgramme = r.DateCompletedProgramme,
          DateOfBmiAtRegistration = r.DateOfBmiAtRegistration,
          DateOfProviderContactedServiceUser =
            r.DateOfProviderContactedServiceUser,
          DateOfProviderSelection = r.DateOfProviderSelection,
          DateOfReferral = r.DateOfReferral,
          DateStartedProgramme = r.DateStartedProgramme,
          DatePlacedOnWaitingList = r.DatePlacedOnWaitingList,
          DateToDelayUntil = r.DateToDelayUntil,
          Deprivation = r.Deprivation,
          DocumentVersion = r.DocumentVersion,
          Ethnicity = r.Ethnicity,
          ServiceUserEthnicityGroup = r.ServiceUserEthnicityGroup,
          ServiceUserEthnicity = r.ServiceUserEthnicity,
          GpRecordedWeight = r.WeightKg,
          HasALearningDisability = r.HasALearningDisability,
          HasAPhysicalDisability = r.HasAPhysicalDisability,
          HasDiabetesType1 = r.HasDiabetesType1,
          HasDiabetesType2 = r.HasDiabetesType2,
          HasHypertension = r.HasHypertension,
          HasRegisteredSeriousMentalIllness =
            r.HasRegisteredSeriousMentalIllness,
          HeightCm = r.HeightCm,
          Id = r.Id,
          IsActive = r.IsActive,
          IsVulnerable = r.IsVulnerable,
          MethodOfContact = r.MethodOfContact == null
            ? MethodOfContact.NoContact.ToString()
            : ((MethodOfContact)r.MethodOfContact.Value).ToString(),
          ModifiedByUserId = r.ModifiedByUserId,
          NumberOfContacts = r.NumberOfContacts,
          OpcsCodes = r.OpcsCodes,
          ProgrammeOutcome = r.ProgrammeOutcome,
          ProviderName = r.Provider?.Name,
          ProviderSubmissions = r.ProviderSubmissions
            .Select(ps => new Models.ProviderSubmission
            {
              Coaching = ps.Coaching,
              Date = ps.Date,
              Measure = ps.Measure,
              SubmissionDate = ps.ModifiedAt,
              Weight = ps.Weight
            })
            .ToList(),
          ReferralLetterDate = r.ReferralLetterDate,
          ReferralSource = r.ReferralSource,
          ReferringGpPracticeNumber = r.ReferringGpPracticeNumber,
          ServiceId = r.ServiceId,
          Sex = r.Sex.IsValidSexString() ? r.Sex : null,
          SourceSystem = r.SourceSystem.ToString(),
          Status = r.Status,
          StatusReason = r.StatusReason,
          TriagedCompletionLevel =
            string.IsNullOrWhiteSpace(r.TriagedCompletionLevel)
              ? null
              : int.Parse(r.TriagedCompletionLevel),
          Ubrn = r.Ubrn,
          VulnerableDescription = r.VulnerableDescription
            .TryParseToAnonymous(),
          StaffRole = r.ReferralSource ==
            ReferralSource.SelfReferral.ToString()
              ? r.StaffRole
              : null,
          ConsentForGpAndNhsNumberLookup = r.ConsentForGpAndNhsNumberLookup,
          ConsentForReferrerUpdatedWithOutcome =
            r.ConsentForReferrerUpdatedWithOutcome,
          ReferringOrganisationOdsCode = r.ReferringOrganisationOdsCode,
          HasActiveEatingDisorder = r.HasActiveEatingDisorder,
          HasArthritisOfKnee = r.HasArthritisOfKnee,
          HasArthritisOfHip = r.HasArthritisOfHip,
          IsPregnant = r.IsPregnant,
          HasHadBariatricSurgery = r.HasHadBariatricSurgery,
          ProviderUbrn = r.ProviderUbrn
        })
        .ToList();

      return anonymisedReferralList;
    }
    catch (Exception)
    {
      throw;
    }
  }

  public async Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferralsByProviderSubmissionsModifiedAt(
    DateTimeOffset lastModifiedDate)
  {
    return await GetAnonymisedReferralsProviderSubmissionsForPredicateAsync(
      (ProviderSubmission r) => r.ModifiedAt >= lastModifiedDate);
  }

  public async Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferralsBySubmissionDate(
    DateTimeOffset? fromDate = null,
    DateTimeOffset? toDate = null)
  {
    fromDate ??= new DateTimeOffset();
    toDate ??= DateTimeOffset.Now;

    return await GetAnonymisedReferralsProviderSubmissionsForPredicateAsync(
      (ProviderSubmission r) => r.Date >= fromDate && r.Date <= toDate);
  }

  /// <inheritdoc/>
  public async Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferralsChangedFromDate(
    DateTimeOffset fromDate)
  {
    IQueryable<Guid> modifiedReferrals = _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.ModifiedAt >= fromDate)
      .Select(r => r.Id);

    IQueryable<Guid> referralsWithNewSubmissions = _context.Referrals
      .Where(r => r.IsActive)
      .Where(r => r.ProviderSubmissions.Any(ps => ps.Date >= fromDate))
      .Select(r => r.Id);

    IEnumerable<Guid> referralIds =
      await modifiedReferrals.Union(referralsWithNewSubmissions).ToListAsync();

    return await GetAnonymisedReferralsForPredicateAsync(r => referralIds.Contains(r.Id));
  }

  private async Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferralsForPredicateAsync(
    Expression<Func<Referral, bool>> predicate)
  {
    List<Referral> entities = await GetReferralEntitiesForPredicateAsync(predicate);

    IEnumerable<AnonymisedReferral> anonymisedReferralList = entities
      .Select(r => new AnonymisedReferral
      {
        Age = r.DateOfBirth?.GetAge(),
        CalculatedBmiAtRegistration = r.CalculatedBmiAtRegistration,
        ModifiedAt = r.ModifiedAt,
        ConsentForFutureContactForEvaluation =
          r.ConsentForFutureContactForEvaluation,
        DateCompletedProgramme = r.DateCompletedProgramme,
        DateOfBmiAtRegistration = r.DateOfBmiAtRegistration,
        DateOfProviderContactedServiceUser =
          r.DateOfProviderContactedServiceUser,
        DateOfProviderSelection = r.DateOfProviderSelection,
        DateOfReferral = r.DateOfReferral,
        DatePlacedOnWaitingList = r.DatePlacedOnWaitingList,
        DateStartedProgramme = r.DateStartedProgramme,
        DateToDelayUntil = r.DateToDelayUntil,
        Deprivation = r.Deprivation,
        DocumentVersion = r.DocumentVersion,
        Ethnicity = r.Ethnicity,
        ServiceUserEthnicityGroup = r.ServiceUserEthnicityGroup,
        ServiceUserEthnicity = r.ServiceUserEthnicity,
        GpRecordedWeight = r.WeightKg,
        HasALearningDisability = r.HasALearningDisability,
        HasAPhysicalDisability = r.HasAPhysicalDisability,
        HasDiabetesType1 = r.HasDiabetesType1,
        HasDiabetesType2 = r.HasDiabetesType2,
        HasHypertension = r.HasHypertension,
        HasRegisteredSeriousMentalIllness =
          r.HasRegisteredSeriousMentalIllness,
        HeightCm = r.HeightCm,
        Id = r.Id,
        IsActive = r.IsActive,
        IsVulnerable = r.IsVulnerable,
        MethodOfContact = r.MethodOfContact == null
          ? MethodOfContact.NoContact.ToString()
          : ((MethodOfContact)r.MethodOfContact.Value).ToString(),
        ModifiedByUserId = r.ModifiedByUserId,
        NumberOfContacts = r.NumberOfContacts,
        OpcsCodes = r.OpcsCodes,
        ProgrammeOutcome = r.ProgrammeOutcome,
        ProviderName = r.Provider?.Name,
        ProviderSubmissions = r.ProviderSubmissions
          .Select(ps => new Models.ProviderSubmission
          {
            Coaching = ps.Coaching,
            Date = ps.Date,
            Measure = ps.Measure,
            SubmissionDate = ps.ModifiedAt,
            Weight = ps.Weight
          })
          .ToList(),
        ReferralLetterDate = r.ReferralLetterDate,
        ReferralSource = r.ReferralSource,
        ReferringGpPracticeNumber = r.ReferringGpPracticeNumber,
        ServiceId = r.ServiceId,
        Sex = r.Sex.IsValidSexString() ? r.Sex : null,
        SourceSystem = r.SourceSystem.ToString(),
        Status = r.Status,
        StatusReason = r.StatusReason,
        TriagedCompletionLevel = string.IsNullOrWhiteSpace(r.TriagedCompletionLevel)
          ? null
          : int.Parse(r.TriagedCompletionLevel, CultureInfo.InvariantCulture),
        Ubrn = r.Ubrn,
        VulnerableDescription = r.VulnerableDescription
          .TryParseToAnonymous(),
        StaffRole = r.ReferralSource == ReferralSource.SelfReferral.ToString()
          ? r.StaffRole
          : null,
        ConsentForGpAndNhsNumberLookup = r.ConsentForGpAndNhsNumberLookup,
        ConsentForReferrerUpdatedWithOutcome =
          r.ConsentForReferrerUpdatedWithOutcome,
        ReferringOrganisationOdsCode = r.ReferringOrganisationOdsCode,
        HasActiveEatingDisorder = r.HasActiveEatingDisorder,
        HasArthritisOfKnee = r.HasArthritisOfKnee,
        HasArthritisOfHip = r.HasArthritisOfHip,
        IsPregnant = r.IsPregnant,
        HasHadBariatricSurgery = r.HasHadBariatricSurgery,
        ProviderUbrn = r.ProviderUbrn
      })
      .ToList();

    return anonymisedReferralList;
  }

  /// <inheritdoc/>
  public async Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferralsForUbrn(string ubrn)
  {
    IEnumerable<AnonymisedReferralHistory> list =
      await _context
        .Referrals
        .AsNoTracking()
        .Include(r => r.ProviderSubmissions
          .Where(ps => ps.IsActive))
        .Include(r => r.Provider)
        .Where(r => r.Ubrn == ubrn)
        .OrderBy(o => o.DateOfReferral)
        .Select(r => new AnonymisedReferralHistory
        {
          Age = r.DateOfBirth == null
            ? null
            : r.DateOfBirth.Value.GetAge(DateTimeOffset.Now),
          CalculatedBmiAtRegistration = r.CalculatedBmiAtRegistration,
          ModifiedAt = r.ModifiedAt,
          ConsentForFutureContactForEvaluation =
            r.ConsentForFutureContactForEvaluation,
          DateCompletedProgramme = r.DateCompletedProgramme,
          DateOfBmiAtRegistration = r.DateOfBmiAtRegistration,
          DateOfProviderContactedServiceUser =
            r.DateOfProviderContactedServiceUser,
          DateOfProviderSelection = r.DateOfProviderSelection,
          DateOfReferral = r.DateOfReferral,
          DatePlacedOnWaitingList = r.DatePlacedOnWaitingList,
          DateStartedProgramme = r.DateStartedProgramme,
          DateToDelayUntil = r.DateToDelayUntil,
          Deprivation = r.Deprivation,
          Ethnicity = r.Ethnicity,
          GpRecordedWeight = r.WeightKg.Value,
          HasALearningDisability = r.HasALearningDisability,
          HasAPhysicalDisability = r.HasAPhysicalDisability,
          HasDiabetesType1 = r.HasDiabetesType1,
          HasDiabetesType2 = r.HasDiabetesType2,
          HasHypertension = r.HasHypertension,
          HasRegisteredSeriousMentalIllness =
            r.HasRegisteredSeriousMentalIllness,
          HeightCm = r.HeightCm,
          Id = r.Id,
          IsActive = r.IsActive,
          IsVulnerable = r.IsVulnerable,
          MethodOfContact = r.MethodOfContact == null
            ? MethodOfContact.NoContact.ToString()
            : ((MethodOfContact)r.MethodOfContact.Value).ToString(),
          ModifiedByUserId = r.ModifiedByUserId,
          NumberOfContacts = r.NumberOfContacts,
          OpcsCodes = r.OpcsCodes,
          ProgrammeOutcome = r.ProgrammeOutcome,
          ProviderName = r.Provider == null
            ? null
            : r.Provider.Name,
          ProviderSubmissions = r.ProviderSubmissions
            .Select(ps => new Models.ProviderSubmission
            {
              Coaching = ps.Coaching,
              Date = ps.Date,
              Measure = ps.Measure,
              SubmissionDate = ps.ModifiedAt,
              Weight = ps.Weight
            })
            .ToList(),
          ReferralLetterDate = r.ReferralLetterDate,
          ReferralSource = r.ReferralSource,
          ReferringGpPracticeNumber = r.ReferringGpPracticeNumber,
          Sex = r.Sex.IsValidSexString() ? r.Sex : null,
          Status = r.Status,
          StatusReason = r.StatusReason,
          TriagedCompletionLevel =
            string.IsNullOrWhiteSpace(r.TriagedCompletionLevel)
              ? null
              : int.Parse(r.TriagedCompletionLevel),
          Ubrn = r.Ubrn,
          VulnerableDescription =
            r.VulnerableDescription.TryParseToAnonymous(),
          StaffRole = r.ReferralSource ==
                      ReferralSource.SelfReferral.ToString()
            ? r.StaffRole
            : null,
          ConsentForGpAndNhsNumberLookup = r.ConsentForGpAndNhsNumberLookup,
          ConsentForReferrerUpdatedWithOutcome =
            r.ConsentForReferrerUpdatedWithOutcome,
          ReferringOrganisationOdsCode = r.ReferringOrganisationOdsCode
        })
        .ToListAsync();

    foreach (AnonymisedReferralHistory item in list)
    {
      List<AnonymisedTextMessage> audits = await _context.TextMessagesAudit
        .Where(t => t.ReferralId == item.Id)
        .OrderBy(t => t.ModifiedAt)
        .Select(t => new AnonymisedTextMessage
        {
          Number = t.Number,
          Sent = t.Sent,
          Outcome = t.Outcome
        }).ToListAsync();

      if (audits.Any())
      {
        item.TextMessageHistory = audits;
      }
    }

    return list;
  }

  /// <inheritdoc/>
  public async Task<IEnumerable<ReprocessedReferral>> GetAnonymisedReprocessedReferralsBySubmissionDate(
    DateTimeOffset? fromDate = null,
    DateTimeOffset? toDate = null)
  {
    var query = await _context.ReferralsAudit
      .AsNoTracking()
      .Where(r => r.IsActive)
      .Where(r => r.DateOfReferral == null
        || (r.DateOfReferral >= fromDate && r.DateOfReferral <= toDate))
      .Where(s => s.ReferralSource == ReferralSource.GpReferral.ToString())
      .Where(t => _context.ReferralsAudit.OrderBy(s => s.ModifiedAt)
        .FirstOrDefault(s =>
          s.Ubrn == t.Ubrn
          && (s.Status == ReferralStatus.New.ToString()
           || s.Status == ReferralStatus.Exception.ToString()
           || s.Status == ReferralStatus.RmcCall.ToString())) != null)
      .Select(t => new
      {
        t.Ubrn,
        t.ModifiedAt,
        t.ReferringGpPracticeNumber,
        t.StatusReason,
        t.DateOfReferral,
        t.ReferralLetterDate,
        t.Status
      })
      .ToListAsync();

    List<ReprocessedReferral> reprocessedReferrals = query
      .GroupBy(t => new { t.Ubrn }, (key, g) => g.OrderBy(r => r.ModifiedAt))
      .Select(rg => new ReprocessedReferral
      {
        DateOfReferral = rg.First().DateOfReferral,
        StatusArray = rg.Select(t => t.Status).ToArray(),
        Ubrn = rg.First().Ubrn,
        InitialStatusReason = rg.First().StatusReason,
        CurrentlyCancelledStatusReason = rg.Last().StatusReason,
        ReferringGpPracticeCode = rg.First().ReferringGpPracticeNumber
      })
      .ToList();

    return reprocessedReferrals;
  }

  /// <inheritdoc/>
  public async Task<IEnumerable<ElectiveCarePostError>> GetElectiveCarePostErrorsAsync(
    DateTimeOffset? fromDate,
    DateTimeOffset? toDate,
    string trustOdsCode,
    Guid? trustUserId = null)
  {
    IQueryable<Entities.ElectiveCarePostError> query =
      _context.ElectiveCarePostErrors
      .Where(x => x.ProcessDate >= fromDate.Value)
      .Where(x => x.ProcessDate <= toDate.Value);

    if (!string.IsNullOrWhiteSpace(trustOdsCode))
    {
      query = query.Where(x => x.TrustOdsCode == trustOdsCode);
    }

    if (trustUserId != null && trustUserId != Guid.Empty)
    {
      query = query.Where(x => x.TrustUserId == trustUserId);
    }

    List<Entities.ElectiveCarePostError> results = await query
      .OrderByDescending(x => x.Id)
      .ToListAsync();

    List<ElectiveCarePostError> electiveCarePostErrors =
      _mapper.Map<List<ElectiveCarePostError>>(results);

    return electiveCarePostErrors;
  }

  /// <inheritdoc/>
  public async Task<IEnumerable<ProviderAwaitingStartReferral>> GetProviderAwaitingStartReferralsAsync()
  {
    List<ProviderAwaitingStartReferral> referrals = await _context
      .Referrals
      .Where(t => t.IsActive)
      .Where(t => t.Status == ReferralStatus.ProviderAwaitingStart.ToString())
      .Select(t => new ProviderAwaitingStartReferral
      {
        DateOfProviderSelection = t.DateOfProviderSelection,
        ProviderName = t.Provider.Name,
        ProviderUbrn = t.ProviderUbrn
      })
      .OrderBy(x => x.ProviderName)
        .ThenBy(x => x.DateOfProviderSelection)
      .ToListAsync();

    IEnumerable<string> invalidProviderUbrns = referrals
      .Where(x => x.DateOfProviderSelection == null)
      .Select(x => x.ProviderUbrn)
      .ToArray();

    if (invalidProviderUbrns.Any())
    {
      throw new InvalidOperationException(
        "Found referrals with a DateOfProviderSelection of null, which " +
        "should not be possible for referrals with a status of " +
        $"{ReferralStatus.ProviderAwaitingStart}. Invalid ProviderUbrns: " +
        string.Join(", ", invalidProviderUbrns));
    }

    return referrals;
  }

  public async Task<IEnumerable<ProviderBiData>> GetProviderBiDataAsync(
    DateTimeOffset? fromDate,
    DateTimeOffset? toDate)
  {
    string awaitingStart = ReferralStatus.ProviderAwaitingStart.ToString();

    IEnumerable<ProviderBiData> providerBiData = await _context
      .Providers
      .AsNoTracking()
      .Where(p => p.IsActive)
      .Select(p => new ProviderBiData
      {
        Name = p.Name,
        NoOfReferralsAwaitingAcceptance = p.Referrals
          .Where(r => r.IsActive)
          .Count(r => r.Status == awaitingStart),
        OldestReferralAwaitingAcceptance = p.Referrals
          .Where(r => r.IsActive)
          .Min(r => r.DateOfProviderSelection),
        ProviderId = p.Id
      })
      .ToArrayAsync();

    var requestErrors = await _context
      .RequestResponseLog
      .Where(r => r.Response != "200")
      .Where(r => r.Response != "500")
      .Where(r => r.RequestAt >= fromDate)
      .Where(r => r.RequestAt <= toDate)
      .Where(r => r.UserId != null)
      .GroupBy(r => new { r.UserId, r.RequestAt.Date })
      .Select(g => new
      {
        ProviderId = g.Key.UserId,
        g.Key.Date,
        Count = g.Count()
      })
      .ToArrayAsync();

    foreach (ProviderBiData provider in providerBiData)
    {
      provider.RequestErrors = requestErrors
        .Where(re => re.ProviderId == provider.ProviderId)
        .GroupBy(r => r.Date)
        .Select(g => new ProviderBiDataRequestError
        {
          Date = g.Key,
          NoOfBadRequests = g.Sum(g => g.Count)
        })
        .OrderBy(p => p.Date)
        .ToArray();
    }

    return providerBiData;
  }

  /// <inheritdoc />
  public async Task<IEnumerable<PseudonymisedReferral>> GetPseudonymisedReferralsAsync(
    DateTimeOffset? fromDate,
    DateTimeOffset? toDate)
  {
    if (fromDate is null)
    {
      throw new ArgumentNullException(nameof(fromDate));
    }

    if (toDate is null)
    {
      throw new ArgumentNullException(nameof(toDate));
    }

    return await GetPseudonymisedReferralsForPredicateAsync(
      (Referral r) => r.DateOfReferral >= fromDate && r.DateOfReferral <= toDate);
  }

  public async Task<List<BiQuestionnaire>> GetQuestionnaires(
    DateTimeOffset start,
    DateTimeOffset end)
  {
    return await _context.ReferralQuestionnaires
      .Include(r => r.Referral)
      .Include(r => r.Questionnaire)
      .Where(r => r.IsActive)
      .Where(r => r.Referral.DateOfReferral >= start
        && r.Referral.DateOfReferral <= end)
      .Select(r => new BiQuestionnaire
      {
        Id = r.Id,
        Answers = r.Answers,
        ConsentToShare = r.ConsentToShare,
        Ubrn = r.Referral.Ubrn,
        QuestionnaireType = r.Questionnaire.Type
      })
      .ToListAsync();
  }

  /// <inheritdoc/>
  public async Task<IEnumerable<ReferralCountByMonth>> GetReferralCountsByMonthAsync()
  {
    List<ReferralCountByMonth> referrals = await _context
      .Referrals
      .Where(r => r.IsActive)
      .Where(r => r.DateOfReferral != null)
      .GroupBy(r => new
      {
        r.DateOfReferral.Value.Date.Year,
        r.DateOfReferral.Value.Date.Month
      },
      (yearMonth, refs) => new ReferralCountByMonth
      {
        NumberOfReferrals = refs.Count(),
        YearMonth = new DateTime(yearMonth.Year, yearMonth.Month, 1)
      })
      .ToListAsync();

    return referrals.OrderBy(r => r.YearMonth);
  }

  /// <inheritdoc/>
  public async Task<IEnumerable<BiRmcUserInformation>> GetRmcUsersInformation(
    DateTimeOffset start,
    DateTimeOffset end)
  {
    return await _context.UserActionLogs
      .Where(t => t.RequestAt >= start && t.RequestAt <= end)
      .Where(t => t.Method == Constants.HttpMethod.POST)
      .Where(t => t.Controller == Constants.WebUi.RMC_CONTROLLER)
      .Join(
        _context.UsersStore,
        ual => ual.UserId,
        us => us.Id,
        (ual, us) => new { ual, us })
      .Select(t => new BiRmcUserInformation(
        t.ual.Action,
        t.us.OwnerName,
        t.ual.Request,
        t.ual.RequestAt,
        t.ual.UserId))
      .ToListAsync();
  }

  /// <inheritdoc/>
  public async Task<IEnumerable<TraceIssueReferral>> GetTraceIssueReferralsAsync()
  {

    // Get ReferralAudits with >1 instance of Status = DischargeAwaitingTrace
    // with first/last dates of that status
    var referralAuditsDischargeAwaitingTrace = _context.ReferralsAudit
      .Where(r => r.Status == ReferralStatus.DischargeAwaitingTrace.ToString())
      .GroupBy(r => r.Id)
      .Where(group => group.Count() > 1)
      .Select(group => new
      {
        Id = group.Key,
        FirstDischargeAwaitingTraceDate
          = group.OrderBy(r => r.AuditId).First().ModifiedAt,
        LastDischargeAwaitingTraceDate
          = group.OrderBy(r => r.AuditId).Last().ModifiedAt
      });

    // Filter for entries with an intermediate non-DischargeAwaitingTrace
    // status (i.e. DischargeAwaitingTrace has been set twice)
    IQueryable<Guid> referralAudits = _context.ReferralsAudit
      .Where(r => r.Status != ReferralStatus.DischargeAwaitingTrace.ToString())
      .Join(referralAuditsDischargeAwaitingTrace,
        r => r.Id,
        ra => ra.Id,
        (r, ra) => new
        {
          r.Id,
          ra.FirstDischargeAwaitingTraceDate,
          ra.LastDischargeAwaitingTraceDate,
          r.ModifiedAt
        })
      .Where(r => r.ModifiedAt > r.FirstDischargeAwaitingTraceDate
        && r.ModifiedAt < r.LastDischargeAwaitingTraceDate)
      .GroupBy(r => r.Id)
      .Select(r => r.Key);

    // Return TraceIssueReferral objects for all referrals with matching Ids
    IQueryable<TraceIssueReferral> referrals = _context.Referrals
      .Where(r => r.IsActive)
      .Join(referralAudits,
        r => r.Id,
        i => i,
        (r, i) => new TraceIssueReferral()
        {
          Id = r.Id,
          ReferringGpPracticeNumber = r.ReferringGpPracticeNumber
        })
      .OrderBy(r => r.ReferringGpPracticeNumber);

#if DEBUG
    string q1 = referrals.ToQueryString();
#endif

    return await referrals.ToListAsync();
  }

  /// <inheritdoc />
  public async Task<IEnumerable<UdalExtract>> GetUdalExtractsAsync(DateTime from, DateTime to)
  {
    List<UdalExtract> udalExtracts = await _context
      .UdalExtracts
      .Where(x => x.ModifiedAt >= from)
      .Where(x => x.ModifiedAt <= to)
      .ProjectTo<UdalExtract>(_mapper.ConfigurationProvider)
      .ToListAsync();

    foreach (UdalExtract udalExtract in udalExtracts)
    {
      // Method of contact must be the enum name and not the value.
      if (udalExtract.MethodOfContact != null)
      {
        udalExtract.MethodOfContact = Enum
          .Parse<MethodOfContact>(udalExtract.MethodOfContact).ToString();
      }

      // NHSE requested referral source SelfReferral to be changed to NhsStaffSelfReferral.
      if (udalExtract.ReferralSource == ReferralSource.SelfReferral.ToString())
      {
        udalExtract.ReferralSource = "NhsStaffSelfReferral";
      }
    }

    return udalExtracts;
  }

  /// <inheritdoc />
  public async Task<IEnumerable<NhsNumberTrace>> GetUntracedNhsNumbers()
  {
    List<NhsNumberTraceReferral> referrals = await _context.Referrals
      .AsNoTracking()
      .Where(r => r.IsActive)
      .Where(r => r.NhsNumber == null
        || string.IsNullOrWhiteSpace(r.ReferringGpPracticeNumber)
        || r.ReferringGpPracticeNumber == Constants.UNKNOWN_GP_PRACTICE_NUMBER
        || r.Status == ReferralStatus.DischargeAwaitingTrace.ToString())
      .Where(r => r.DateOfBirth != null)
      .Where(r => r.FamilyName != null)
      .Where(r => r.GivenName != null)
      .Where(r => r.Postcode != null)
      .Where(r => r.DateCompletedProgramme == null ||
        r.DateCompletedProgramme > DateTimeOffset.Now.AddDays(-730))
      .Select(r => new NhsNumberTraceReferral()
      {
        Id = r.Id,
        GivenName = r.GivenName,
        FamilyName = r.FamilyName,
        DateOfBirth = r.DateOfBirth.Value,
        LastTraceDate = r.LastTraceDate,
        Status = r.Status,
        Postcode = r.Postcode,
        TraceCount = r.TraceCount
      })
      .ToListAsync();

    return referrals
      .Where(r => r.Status.CanTraceReferralStatusString<ReferralStatus>())
      .Where(r => r.LastTraceDate == null ||
        r.LastTraceDate < DateTimeOffset.Now.AddDays(-_options.DaysBetweenTraces))
      .Select(r => new NhsNumberTrace
      {
        DateOfBirth = r.DateOfBirth,
        FamilyName = r.FamilyName,
        GivenName = r.GivenName,
        Id = r.Id,
        Postcode = r.Postcode
      });
  }

  /// <inheritdoc/>
  public ProviderEndedData ProviderEndedReasonStats(
    DateTimeOffset? from,
    DateTimeOffset? to)
  {
    var providerEnded = _context
    .Referrals
    .Include(r => r.ProviderSubmissions.Where(ps => ps.IsActive))
    .Where(r => r.IsActive)
    .Where(r => r.DateOfReferral >= from && r.DateOfReferral <= to)
    .Join(
      _context.ReferralsAudit.Where(a =>
        a.Status == ReferralStatus.ProviderDeclinedByServiceUser.ToString()
        || a.Status == ReferralStatus.ProviderDeclinedTextMessage.ToString()
        || a.Status == ReferralStatus.ProviderTerminated.ToString()
        || a.Status == ReferralStatus.ProviderTerminatedTextMessage.ToString()
        || a.Status == ReferralStatus.ProviderRejected.ToString()
        || a.Status == ReferralStatus.ProviderRejectedTextMessage.ToString()),
      src => src.Id,
      audit => audit.Id,
      (src, audit) => new { Audit = audit, Referral = src })
    .AsNoTracking()
    .GroupBy(r => r.Audit.Status)
    .Select(g => new
    {
      Status = g.Key,
      Count = g.Count()
    })
    .ToArray();

    ProviderEndedData providerEndedData = new()
    {
      FromDate = from ?? DateTimeOffset.UtcNow.AddDays(-31),
      ToDate = to ?? DateTimeOffset.UtcNow
    };

    if (providerEnded == null)
    {
      return providerEndedData;
    }

    providerEndedData.Declined = providerEnded.Count(t =>
      t.Status == ReferralStatus.ProviderDeclinedByServiceUser.ToString()
      || t.Status == ReferralStatus.ProviderDeclinedTextMessage.ToString());
    providerEndedData.Rejected = providerEnded.Count(t =>
      t.Status == ReferralStatus.ProviderRejected.ToString()
      || t.Status == ReferralStatus.ProviderRejectedTextMessage.ToString());
    providerEndedData.Terminated = providerEnded.Count(t =>
      t.Status == ReferralStatus.ProviderTerminated.ToString()
      || t.Status == ReferralStatus.ProviderTerminatedTextMessage.ToString());

    return providerEndedData;
  }

  /// <inheritdoc />
  public async Task<List<SpineTraceResponse>> UpdateSpineTraced(
    IEnumerable<SpineTraceResult> spineTraceResults)
  {
    if (spineTraceResults is null)
    {
      _logger.Information("IEnumerable<SpineTraceResult> is null.");
      return SpineTraceResponseError(
        "IEnumerable<SpineTraceResult> is null.");
    }

    List<SpineTraceResponse> responses = new();
    List<Referral> unableToTrace = new();

    foreach (SpineTraceResult traceResult in spineTraceResults)
    {
      SpineTraceResponse response = _mapper
        .Map<SpineTraceResponse>(traceResult);

      ValidateModelResult validationResult = ValidateModel(traceResult);
      if (!validationResult.IsValid)
      {
        string error = validationResult.GetErrorMessage();
        _logger.Information(error);
        response.Errors.Add(error);
        responses.Add(response);

        if (traceResult.HasValidId)
        {
          _logger.Information("Id is valid Guid attempting to proceed.");
        }
        else
        {
          continue;
        }
      }

      Referral referral = await _context.Referrals.FindAsync(traceResult.Id);
      if (referral == null)
      {
        string error = $"Referral not found with an id of {traceResult.Id}.";
        _logger.Information(error);
        response.Errors.Add(error);
        responses.Add(response);
        continue;
      }

      referral.LastTraceDate = DateTimeOffset.Now;
      referral.TraceCount = (referral.TraceCount ?? 0) + 1;

      UpdateModified(referral);
      string previousGpPracticeNumber = referral.ReferringGpPracticeNumber;

      if (traceResult.IsTraceSuccessful && validationResult.IsValid)
      {
        referral.ReferringGpPracticeNumber = traceResult.GpPracticeOdsCode;
        referral.ReferringGpPracticeName = traceResult.GpPracticeName;
        referral.NhsNumber = traceResult.NhsNumber;

        if (referral.TraceCount == 1)
        {
          List<Referral> matchedReferrals = await _context.Referrals
            .Where(r => r.IsActive)
            .Where(r => r.Id != referral.Id)
            .Where(r => r.NhsNumber == referral.NhsNumber)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

          try
          {
            ReferralHelper.CheckMatchingReferralsIfReEntryIsAllowed(
              matchedReferrals);
          }
          catch (InvalidOperationException ex)
          {
            _logger.Warning(ex, ex.Message);
            response.Errors.Add(ex.Message);
          }
          catch (ReferralNotUniqueException ex)
          {
            referral.Status =
              ReferralStatus.CancelledDuplicateTextMessage.ToString();
            referral.StatusReason = ex.Message +
              " The selected provider has been removed.";
            referral.ProviderId = null;
            referral.DateOfProviderSelection = null;
            response.Errors.Add(
              $"Status: {referral.Status}. Reason: {referral.StatusReason}");
          }
          catch (Exception ex)
          {
            _logger.Error(ex, ex.Message);
            response.Errors.Add(ex.Message);
          }
        }
      }
      else
      {
        referral.ReferringGpPracticeNumber =
         Constants.UNKNOWN_GP_PRACTICE_NUMBER;
        referral.ReferringGpPracticeName =
          Constants.UNKNOWN_GP_PRACTICE_NAME;
      }

      if (referral.Status == ReferralStatus.ProviderAwaitingTrace.ToString())
      {
        referral.Status = ReferralStatus.ProviderAwaitingStart.ToString();
      }
      else if (referral.Status ==
        ReferralStatus.DischargeAwaitingTrace.ToString())
      {
        if (referral.ReferringGpPracticeNumber == previousGpPracticeNumber
          || Constants.UNKNOWN_ODS_CODES
            .Contains(referral.ReferringGpPracticeNumber))
        {
          unableToTrace.Add(referral);
        }
        else
        {
          referral.Status = ReferralStatus.AwaitingDischarge.ToString();
        }
      }

      responses.Add(response);
    }

    await _context.SaveChangesAsync();

    if (unableToTrace.Any())
    {
      foreach (Referral referral in unableToTrace)
      {
        referral.Status = ReferralStatus.UnableToDischarge.ToString();
        referral.StatusReason = Constants.UNABLE_TO_TRACE_STATUS_REASON;
      }

      await _context.SaveChangesAsync();

      foreach (Referral referral in unableToTrace)
      {
        referral.Status = ReferralStatus.Complete.ToString();
        referral.StatusReason = null;
      }

      await _context.SaveChangesAsync();
    }

    return responses;
  }

  private async Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferralsProviderSubmissionsForPredicateAsync(
    Expression<Func<ProviderSubmission, bool>> predicate)
  {
    List<ProviderSubmission> submissions1 = await _context
      .ProviderSubmissions
      .AsNoTracking()
      .Where(predicate)
      .Where(ps => ps.IsActive)
      .ToListAsync();

    List<ProviderSubmission> submissions = submissions1
      .GroupBy(ps =>
        new { ps.Coaching, ps.Date, ps.Measure, ps.ReferralId, ps.Weight })
      .Select(g => new ProviderSubmission
      {
        Coaching = g.Key.Coaching,
        Date = g.Key.Date,
        Measure = g.Key.Measure,
        ModifiedAt = g.Max(ps => ps.ModifiedAt),
        ReferralId = g.Key.ReferralId,
        Weight = g.Key.Weight
      })
      .ToList();

    Guid[] referralIds = submissions
      .Select(s => s.ReferralId).Distinct().ToArray();

    List<Referral> entities = _context
      .Referrals
      .AsNoTracking()
      .Where(r => referralIds.Contains(r.Id))
      .Select(r => new Referral
      {
        DateOfBirth = r.DateOfBirth,
        CalculatedBmiAtRegistration = r.CalculatedBmiAtRegistration,
        ModifiedAt = r.ModifiedAt,
        ConsentForFutureContactForEvaluation =
          r.ConsentForFutureContactForEvaluation,
        DateCompletedProgramme = r.DateCompletedProgramme,
        DateOfBmiAtRegistration = r.DateOfBmiAtRegistration,
        DateOfProviderContactedServiceUser =
          r.DateOfProviderContactedServiceUser,
        DateOfProviderSelection = r.DateOfProviderSelection,
        DateOfReferral = r.DateOfReferral,
        DatePlacedOnWaitingList = r.DatePlacedOnWaitingList,
        DateStartedProgramme = r.DateStartedProgramme,
        DateToDelayUntil = r.DateToDelayUntil,
        Deprivation = r.Deprivation,
        Ethnicity = r.Ethnicity,
        ServiceUserEthnicityGroup = r.ServiceUserEthnicityGroup,
        ServiceUserEthnicity = r.ServiceUserEthnicity,
        WeightKg = r.WeightKg,
        HasALearningDisability = r.HasALearningDisability,
        HasAPhysicalDisability = r.HasAPhysicalDisability,
        HasDiabetesType1 = r.HasDiabetesType1,
        HasDiabetesType2 = r.HasDiabetesType2,
        HasHypertension = r.HasHypertension,
        HasRegisteredSeriousMentalIllness =
          r.HasRegisteredSeriousMentalIllness,
        HeightCm = r.HeightCm,
        Id = r.Id,
        IsActive = r.IsActive,
        IsVulnerable = r.IsVulnerable,
        MethodOfContact = r.MethodOfContact,
        ModifiedByUserId = r.ModifiedByUserId,
        NumberOfContacts = r.NumberOfContacts,
        OpcsCodes = r.OpcsCodes,
        ProgrammeOutcome = r.ProgrammeOutcome,
        Provider = new Provider
        {
          Name = r.Provider.Name
        },
        ReferralLetterDate = r.ReferralLetterDate,
        ReferralSource = r.ReferralSource,
        ReferringGpPracticeNumber = r.ReferringGpPracticeNumber,
        Sex = r.Sex,
        Status = r.Status,
        StatusReason = r.StatusReason,
        TriagedCompletionLevel = r.TriagedCompletionLevel,
        Ubrn = r.Ubrn,
        VulnerableDescription = r.VulnerableDescription,
        StaffRole = r.ReferralSource,
        ConsentForGpAndNhsNumberLookup = r.ConsentForGpAndNhsNumberLookup,
        ConsentForReferrerUpdatedWithOutcome =
          r.ConsentForReferrerUpdatedWithOutcome,
        ReferringOrganisationEmail = r.ReferringOrganisationEmail,
        ReferringOrganisationOdsCode = r.ReferringOrganisationOdsCode,
        HasActiveEatingDisorder = r.HasActiveEatingDisorder,
        HasArthritisOfKnee = r.HasArthritisOfKnee,
        HasArthritisOfHip = r.HasArthritisOfHip,
        IsPregnant = r.IsPregnant,
        HasHadBariatricSurgery = r.HasHadBariatricSurgery,
        ProviderUbrn = r.ProviderUbrn
      })
      .OrderBy(r => r.DateOfReferral)
      .ToList();

    foreach (Referral entity in entities)
    {
      entity.InjectionRemover();
      entity.ProviderSubmissions = submissions
        .Where(s => s.ReferralId == entity.Id).ToList();
    }

    IEnumerable<AnonymisedReferral> anonymisedReferralList = entities
      .Select(r => new AnonymisedReferral
      {
        Age = r.DateOfBirth?.GetAge(),
        CalculatedBmiAtRegistration = r.CalculatedBmiAtRegistration,
        ModifiedAt = r.ModifiedAt,
        ConsentForFutureContactForEvaluation =
          r.ConsentForFutureContactForEvaluation,
        DateCompletedProgramme = r.DateCompletedProgramme,
        DateOfBmiAtRegistration = r.DateOfBmiAtRegistration,
        DateOfProviderContactedServiceUser =
          r.DateOfProviderContactedServiceUser,
        DateOfProviderSelection = r.DateOfProviderSelection,
        DateOfReferral = r.DateOfReferral,
        DatePlacedOnWaitingList = r.DatePlacedOnWaitingList,
        DateStartedProgramme = r.DateStartedProgramme,
        DateToDelayUntil = r.DateToDelayUntil,
        Deprivation = r.Deprivation,
        Ethnicity = r.Ethnicity,
        ServiceUserEthnicityGroup = r.ServiceUserEthnicityGroup,
        ServiceUserEthnicity = r.ServiceUserEthnicity,
        GpRecordedWeight = r.WeightKg,
        HasALearningDisability = r.HasALearningDisability,
        HasAPhysicalDisability = r.HasAPhysicalDisability,
        HasDiabetesType1 = r.HasDiabetesType1,
        HasDiabetesType2 = r.HasDiabetesType2,
        HasHypertension = r.HasHypertension,
        HasRegisteredSeriousMentalIllness =
          r.HasRegisteredSeriousMentalIllness,
        HeightCm = r.HeightCm,
        Id = r.Id,
        IsActive = r.IsActive,
        IsVulnerable = r.IsVulnerable,
        MethodOfContact = r.MethodOfContact == null
          ? MethodOfContact.NoContact.ToString()
          : ((MethodOfContact)r.MethodOfContact.Value).ToString(),
        ModifiedByUserId = r.ModifiedByUserId,
        NumberOfContacts = r.NumberOfContacts,
        OpcsCodes = r.OpcsCodes,
        ProgrammeOutcome = r.ProgrammeOutcome,
        ProviderName = r.Provider?.Name,
        ProviderSubmissions = r.ProviderSubmissions
          .Select(ps => new Models.ProviderSubmission
          {
            Coaching = ps.Coaching,
            Date = ps.Date,
            Measure = ps.Measure,
            SubmissionDate = ps.ModifiedAt,
            Weight = ps.Weight
          })
          .ToList(),
        ReferralLetterDate = r.ReferralLetterDate,
        ReferralSource = r.ReferralSource,
        ReferringGpPracticeNumber = r.ReferringGpPracticeNumber,
        Sex = r.Sex,
        Status = r.Status,
        StatusReason = r.StatusReason,
        TriagedCompletionLevel =
          string.IsNullOrWhiteSpace(r.TriagedCompletionLevel)
            ? null
            : int.Parse(r.TriagedCompletionLevel),
        Ubrn = r.Ubrn,
        VulnerableDescription = r.VulnerableDescription.TryParseToAnonymous(),
        StaffRole =
          r.ReferralSource == ReferralSource.SelfReferral.ToString()
            ? r.StaffRole
            : null,
        ConsentForGpAndNhsNumberLookup = r.ConsentForGpAndNhsNumberLookup,
        ConsentForReferrerUpdatedWithOutcome =
          r.ConsentForReferrerUpdatedWithOutcome,
        ReferringOrganisationOdsCode = r.ReferringOrganisationOdsCode,
        HasActiveEatingDisorder = r.HasActiveEatingDisorder,
        HasArthritisOfKnee = r.HasArthritisOfKnee,
        HasArthritisOfHip = r.HasArthritisOfHip,
        IsPregnant = r.IsPregnant,
        HasHadBariatricSurgery = r.HasHadBariatricSurgery,
        ProviderUbrn = r.ProviderUbrn
      })
      .ToList();

    return anonymisedReferralList;
  }

  private async Task<IEnumerable<PseudonymisedReferral>> GetPseudonymisedReferralsForPredicateAsync(
    Expression<Func<Referral, bool>> predicate)
  {
    List<Referral> entities = await GetReferralEntitiesForPredicatePseudonymisedAsync(predicate);

    IEnumerable<PseudonymisedReferral> pseudonymisedReferralList = entities
      .Select(r => new PseudonymisedReferral
      {
        Age = r.DateOfBirth?.GetAge(),
        CalculatedBmiAtRegistration = r.CalculatedBmiAtRegistration,
        ConsentForFutureContactForEvaluation = r.ConsentForFutureContactForEvaluation,
        ConsentForGpAndNhsNumberLookup = r.ConsentForGpAndNhsNumberLookup,
        ConsentForReferrerUpdatedWithOutcome = r.ConsentForReferrerUpdatedWithOutcome,
        DateOfBmiAtRegistration = r.DateOfBmiAtRegistration,
        DateOfProviderContactedServiceUser = r.DateOfProviderContactedServiceUser,
        DateOfProviderSelection = r.DateOfProviderSelection,
        DateOfReferral = r.DateOfReferral,
        DatePlacedOnWaitingList = r.DatePlacedOnWaitingList,
        DateToDelayUntil = r.DateToDelayUntil,
        Deprivation = r.Deprivation,
        DocumentVersion = r.DocumentVersion,
        Ethnicity = r.Ethnicity,
        GpRecordedWeight = r.WeightKg,
        HasActiveEatingDisorder = r.HasActiveEatingDisorder,
        HasALearningDisability = r.HasALearningDisability,
        HasAPhysicalDisability = r.HasAPhysicalDisability,
        HasArthritisOfHip = r.HasArthritisOfHip,
        HasArthritisOfKnee = r.HasArthritisOfKnee,
        HasDiabetesType1 = r.HasDiabetesType1,
        HasDiabetesType2 = r.HasDiabetesType2,
        HasHadBariatricSurgery = r.HasHadBariatricSurgery,
        HasHypertension = r.HasHypertension,
        HasRegisteredSeriousMentalIllness = r.HasRegisteredSeriousMentalIllness,
        HeightCm = r.HeightCm,
        Id = r.Id,
        IsActive = r.IsActive,
        IsPregnant = r.IsPregnant,
        IsVulnerable = r.IsVulnerable,
        MethodOfContact = r.MethodOfContact == null
          ? MethodOfContact.NoContact.ToString()
          : ((MethodOfContact)r.MethodOfContact.Value).ToString(),
        ModifiedAt = r.ModifiedAt,
        ModifiedByUserId = r.ModifiedByUserId,
        NhsNumber = r.NhsNumber,
        NumberOfContacts = r.NumberOfContacts,
        OfferedCompletionLevel = r.OfferedCompletionLevel,
        OpcsCodes = r.OpcsCodes,
        ProviderName = r.Provider?.Name,
        ProviderSubmissions = r.ProviderSubmissions
          .Select(ps => new Models.ProviderSubmission
          {
            Coaching = ps.Coaching
          })
          .ToList(),
        ProviderUbrn = r.ProviderUbrn,
        ReferralLetterDate = r.ReferralLetterDate,
        ReferralSource = r.ReferralSource,
        ReferringGpPracticeNumber = r.ReferringGpPracticeNumber,
        ReferringOrganisationOdsCode = r.ReferringOrganisationOdsCode,
        ServiceId = r.ServiceId,
        ServiceUserEthnicity = r.ServiceUserEthnicity,
        ServiceUserEthnicityGroup = r.ServiceUserEthnicityGroup,
        Sex = r.Sex,
        SourceSystem = r.SourceSystem.ToString(),
        StaffRole = r.ReferralSource == ReferralSource.SelfReferral.ToString() ? r.StaffRole : null,
        Status = r.Status,
        StatusReason = r.StatusReason,
        TriagedCompletionLevel = string.IsNullOrWhiteSpace(r.TriagedCompletionLevel)
          ? null
          : int.Parse(r.TriagedCompletionLevel, CultureInfo.InvariantCulture),
        Ubrn = r.Ubrn,
        VulnerableDescription = r.VulnerableDescription.TryParseToAnonymous()
      })
      .ToList();

    return pseudonymisedReferralList;
  }

  private async Task<List<Referral>> GetReferralEntitiesForPredicateAsync(
    Expression<Func<Referral, bool>> predicate)
  {
    List<Referral> entities = await _context
      .Referrals
      .AsNoTracking()
      .Where(r => r.IsActive)
      .Include(r => r.ProviderSubmissions
        .Where(ps => ps.IsActive))
      .Include(r => r.Provider)
      .Where(predicate)
      .Select(r => new Referral
      {
        DateOfBirth = r.DateOfBirth,
        CalculatedBmiAtRegistration = r.CalculatedBmiAtRegistration,
        ModifiedAt = r.ModifiedAt,
        ConsentForFutureContactForEvaluation =
          r.ConsentForFutureContactForEvaluation,
        DateCompletedProgramme = r.DateCompletedProgramme,
        DateOfBmiAtRegistration = r.DateOfBmiAtRegistration,
        DateOfProviderContactedServiceUser =
          r.DateOfProviderContactedServiceUser,
        DateOfProviderSelection = r.DateOfProviderSelection,
        DateOfReferral = r.DateOfReferral,
        DatePlacedOnWaitingList = r.DatePlacedOnWaitingList,
        DateStartedProgramme = r.DateStartedProgramme,
        DateToDelayUntil = r.DateToDelayUntil,
        Deprivation = r.Deprivation,
        Ethnicity = r.Ethnicity,
        ServiceUserEthnicityGroup = r.ServiceUserEthnicityGroup,
        ServiceUserEthnicity = r.ServiceUserEthnicity,
        WeightKg = r.WeightKg,
        HasALearningDisability = r.HasALearningDisability,
        HasAPhysicalDisability = r.HasAPhysicalDisability,
        HasDiabetesType1 = r.HasDiabetesType1,
        HasDiabetesType2 = r.HasDiabetesType2,
        HasHypertension = r.HasHypertension,
        HasRegisteredSeriousMentalIllness =
          r.HasRegisteredSeriousMentalIllness,
        HeightCm = r.HeightCm,
        Id = r.Id,
        IsActive = r.IsActive,
        IsVulnerable = r.IsVulnerable,
        MethodOfContact = r.MethodOfContact,
        ModifiedByUserId = r.ModifiedByUserId,
        NhsNumber = r.NhsNumber,
        NumberOfContacts = r.NumberOfContacts,
        OpcsCodes = r.OpcsCodes,
        ProgrammeOutcome = r.ProgrammeOutcome,
        Provider = new Provider
        {
          Name = r.Provider.Name
        },
        ProviderSubmissions = r.ProviderSubmissions
          .GroupBy(ps => new
          {
            ps.Coaching,
            ps.Date,
            ps.Measure,
            ps.ReferralId,
            ps.Weight,
            ps.Id
          })
          .Select(g => new ProviderSubmission
          {
            Coaching = g.Key.Coaching,
            Date = g.Key.Date,
            Id = g.Key.Id,
            Measure = g.Key.Measure,
            ModifiedAt = g.Max(ps => ps.ModifiedAt),
            ReferralId = g.Key.ReferralId,
            Weight = g.Key.Weight
          })
          .ToList(),
        ReferralLetterDate = r.ReferralLetterDate,
        ReferralSource = r.ReferralSource,
        ReferringGpPracticeNumber = r.ReferringGpPracticeNumber,
        Sex = r.Sex,
        Status = r.Status,
        StatusReason = r.StatusReason,
        TriagedCompletionLevel = r.TriagedCompletionLevel,
        Ubrn = r.Ubrn,
        VulnerableDescription = r.VulnerableDescription,
        StaffRole = r.StaffRole,
        ConsentForGpAndNhsNumberLookup = r.ConsentForGpAndNhsNumberLookup,
        ConsentForReferrerUpdatedWithOutcome =
          r.ConsentForReferrerUpdatedWithOutcome,
        ReferringOrganisationEmail = r.ReferringOrganisationEmail,
        ReferringOrganisationOdsCode = r.ReferringOrganisationOdsCode,
        HasActiveEatingDisorder = r.HasActiveEatingDisorder,
        HasArthritisOfKnee = r.HasArthritisOfKnee,
        HasArthritisOfHip = r.HasArthritisOfHip,
        IsPregnant = r.IsPregnant,
        HasHadBariatricSurgery = r.HasHadBariatricSurgery,
        SourceSystem = r.SourceSystem,
        DocumentVersion = r.DocumentVersion,
        ServiceId = r.ServiceId,
        ProviderUbrn = r.ProviderUbrn
      })
      .OrderBy(o => o.DateOfReferral)
      .ToListAsync();

    foreach (Referral entity in entities)
    {
      entity.InjectionRemover();
    }

    return entities;
  }

  private async Task<List<Referral>> GetReferralEntitiesForPredicatePseudonymisedAsync(
    Expression<Func<Referral, bool>> predicate)
  {
    List<Referral> entities = await _context
      .Referrals
      .AsNoTracking()
      .Where(r => r.IsActive)
      .Where(predicate)
      .Select(r => new Referral
      {
        DateOfBirth = r.DateOfBirth,
        CalculatedBmiAtRegistration = r.CalculatedBmiAtRegistration,
        ConsentForFutureContactForEvaluation = r.ConsentForFutureContactForEvaluation,
        ConsentForGpAndNhsNumberLookup = r.ConsentForGpAndNhsNumberLookup,
        ConsentForReferrerUpdatedWithOutcome = r.ConsentForReferrerUpdatedWithOutcome,
        DateOfBmiAtRegistration = r.DateOfBmiAtRegistration,
        DateOfProviderContactedServiceUser = r.DateOfProviderContactedServiceUser,
        DateOfProviderSelection = r.DateOfProviderSelection,
        DateOfReferral = r.DateOfReferral,
        DatePlacedOnWaitingList = r.DatePlacedOnWaitingList,
        DateToDelayUntil = r.DateToDelayUntil,
        Deprivation = r.Deprivation,
        DocumentVersion = r.DocumentVersion,
        Ethnicity = r.Ethnicity,
        HasActiveEatingDisorder = r.HasActiveEatingDisorder,
        HasALearningDisability = r.HasALearningDisability,
        HasAPhysicalDisability = r.HasAPhysicalDisability,
        HasArthritisOfKnee = r.HasArthritisOfKnee,
        HasArthritisOfHip = r.HasArthritisOfHip,
        HasDiabetesType1 = r.HasDiabetesType1,
        HasDiabetesType2 = r.HasDiabetesType2,
        HasHadBariatricSurgery = r.HasHadBariatricSurgery,
        HasHypertension = r.HasHypertension,
        HasRegisteredSeriousMentalIllness = r.HasRegisteredSeriousMentalIllness,
        HeightCm = r.HeightCm,
        Id = r.Id,
        IsActive = r.IsActive,
        IsPregnant = r.IsPregnant,
        IsVulnerable = r.IsVulnerable,
        MethodOfContact = r.MethodOfContact,
        ModifiedAt = r.ModifiedAt,
        ModifiedByUserId = r.ModifiedByUserId,
        NhsNumber = r.NhsNumber,
        NumberOfContacts = r.NumberOfContacts,
        OfferedCompletionLevel = r.OfferedCompletionLevel,
        OpcsCodes = r.OpcsCodes,
        Provider = new Provider
        {
          Name = r.Provider.Name
        },
        ProviderUbrn = r.ProviderUbrn,
        ProviderSubmissions = r.ProviderSubmissions
          .Where(g => g.Coaching > 0)
          .Select(g => new ProviderSubmission
          {
            Coaching = g.Coaching
          })
          .ToList(),
        ReferralLetterDate = r.ReferralLetterDate,
        ReferralSource = r.ReferralSource,
        ReferringGpPracticeNumber = r.ReferringGpPracticeNumber,
        ReferringOrganisationOdsCode = r.ReferringOrganisationOdsCode,
        ServiceId = r.ServiceId,
        ServiceUserEthnicity = r.ServiceUserEthnicity,
        ServiceUserEthnicityGroup = r.ServiceUserEthnicityGroup,
        Sex = r.Sex,
        SourceSystem = r.SourceSystem,
        StaffRole = r.StaffRole,
        Status = r.Status,
        StatusReason = r.StatusReason,
        TriagedCompletionLevel = r.TriagedCompletionLevel,
        Ubrn = r.Ubrn,
        VulnerableDescription = r.VulnerableDescription,
        WeightKg = r.WeightKg
      })
      .OrderBy(o => o.DateOfReferral)
      .ToListAsync();

    foreach (Referral entity in entities)
    {
      entity.InjectionRemover();
    }

    return entities;
  }

  private List<SpineTraceResponse> SpineTraceResponseError(string error) =>
    new() { new SpineTraceResponse(error) };
}
