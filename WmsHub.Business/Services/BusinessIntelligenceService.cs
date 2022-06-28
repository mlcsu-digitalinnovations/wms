using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Extensions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Services.Interfaces;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using Referral = WmsHub.Business.Entities.Referral;

namespace WmsHub.Business.Services
{
  public class BusinessIntelligenceService
    : ServiceBase<Entities.Referral>, IBusinessIntelligenceService
  {
    private readonly IMapper _mapper;

    public BusinessIntelligenceService(
      DatabaseContext context,
      IMapper mapper,
      ILogger log)
      : base(context)
    {
      _mapper = mapper;
      Log.Logger = log;
    }

    public async Task<IEnumerable<AnonymisedReferral>> GetAnonymisedReferrals
      (DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null)
    {
      fromDate ??= new DateTimeOffset();
      toDate ??= DateTimeOffset.Now;

      List<Referral> entities = await _context
        .Referrals
        .AsNoTracking()
        .Where(r => r.IsActive)
        .Include(r => r.ProviderSubmissions
          .Where(ps => ps.IsActive))
        .Include(r => r.Provider)
        .Where(r => r.DateOfReferral >= fromDate)
        .Where(r => r.DateOfReferral <= toDate)
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
          DateStartedProgramme = r.DateStartedProgramme,
          DateToDelayUntil = r.DateToDelayUntil,
          Deprivation = r.Deprivation,
          Ethnicity = r.Ethnicity,
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
          ProgrammeOutcome = r.ProgrammeOutcome,
          Provider = new Entities.Provider
          {
            Name = r.Provider.Name
          },
          ProviderSubmissions = r.ProviderSubmissions
            .GroupBy(ps =>
              new { ps.Coaching, ps.Date, ps.Measure, ps.ReferralId, ps.Weight, ps.Id })
            .Select(g => new Entities.ProviderSubmission()
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
          
        })
        .OrderBy(o => o.DateOfReferral)
        .ToListAsync();

      foreach (var entity in entities)
      {
        entity.InjectionRemover();
      }

      IEnumerable<AnonymisedReferral> anonymisedReferralList = entities
        .Select(r => new AnonymisedReferral()
        {
          Age = r.DateOfBirth == null
            ? null
            : r.DateOfBirth.Value.GetAge(),
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
          DateToDelayUntil = r.DateToDelayUntil,
          Deprivation = r.Deprivation,
          DocumentVersion = r.DocumentVersion,
          Ethnicity = r.Ethnicity,
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
            : ((MethodOfContact)(r.MethodOfContact.Value)).ToString(),
          ModifiedByUserId = r.ModifiedByUserId,
          NumberOfContacts = r.NumberOfContacts == null
            ? 0
            : r.NumberOfContacts.Value,
          ProgrammeOutcome = r.ProgrammeOutcome,
          ProviderName = r.Provider == null
            ? null
            : r.Provider.Name,
          ProviderSubmissions = r.ProviderSubmissions
            .Select(ps => new ProviderSubmission()
            {
              Coaching = ps.Coaching,
              Date = ps.Date,
              Measure = ps.Measure,
              SubmissionDate = ps.ModifiedAt,
              Weight = ps.Weight
            })
            .ToList(),
          ReferralSource = r.ReferralSource,
          ReferringGpPracticeNumber = r.ReferringGpPracticeNumber,
          ServiceId = r.ServiceId,
          Sex = r.Sex.TryParseSex(),
          SourceSystem = r.SourceSystem.ToString(),
          Status = r.Status,
          StatusReason = r.StatusReason,
          TriagedCompletionLevel =
            string.IsNullOrWhiteSpace(r.TriagedCompletionLevel)
              ? null
              : int.Parse(r.TriagedCompletionLevel),
          Ubrn = r.Ubrn,
          VulnerableDescription = r.VulnerableDescription.TryParseToAnonymous(),
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
          HasHadBariatricSurgery = r.HasHadBariatricSurgery
        })
        .ToList();

      return anonymisedReferralList;
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
          Date = g.Key.Date,
          Count = g.Count()
        })
        .ToArrayAsync();

      foreach (var provider in providerBiData)
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

    public async Task<IEnumerable<AnonymisedReferral>>
      GetAnonymisedReferralsBySubmissionDate(
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null)
    {

      fromDate ??= new DateTimeOffset();
      toDate ??= DateTimeOffset.Now;

      Entities.ProviderSubmission[] submissions = await _context
        .ProviderSubmissions
        .AsNoTracking()
        .Where(ps => ps.Date >= fromDate)
        .Where(ps => ps.Date <= toDate)
        .Where(ps => ps.IsActive)
        .GroupBy(ps => 
          new { ps.Coaching, ps.Date, ps.Measure, ps.ReferralId, ps.Weight })
        .Select(g => new Entities.ProviderSubmission()
        {
          Coaching = g.Key.Coaching,
          Date = g.Key.Date,
          Measure = g.Key.Measure,
          ModifiedAt = g.Max(ps => ps.ModifiedAt),
          ReferralId = g.Key.ReferralId,
          Weight = g.Key.Weight
        })
        .ToArrayAsync();

      var referralIds = submissions
        .Select(s => s.ReferralId).Distinct().ToArray();

      List<Referral> enitities = await _context
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
          DateStartedProgramme = r.DateStartedProgramme,
          DateToDelayUntil = r.DateToDelayUntil,
          Deprivation = r.Deprivation,
          Ethnicity = r.Ethnicity,
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
          ProgrammeOutcome = r.ProgrammeOutcome,
          Provider = new Entities.Provider
          {
            Name = r.Provider.Name
          },
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
          HasHadBariatricSurgery = r.HasHadBariatricSurgery
        })
        .OrderBy(r => r.DateOfReferral)
        .ToListAsync();

      foreach (var entity in enitities)
      {
        entity.InjectionRemover();
        entity.ProviderSubmissions = submissions
          .Where(s => s.ReferralId == entity.Id).ToList();
      }

      IEnumerable<AnonymisedReferral> anonymisedReferralList = enitities
        .Select(r => new AnonymisedReferral()
        {
          Age = r.DateOfBirth == null
            ? null
            : r.DateOfBirth.Value.GetAge(),
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
          DateToDelayUntil = r.DateToDelayUntil,
          Deprivation = r.Deprivation,
          Ethnicity = r.Ethnicity,
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
            : ((MethodOfContact)(r.MethodOfContact.Value)).ToString(),
          ModifiedByUserId = r.ModifiedByUserId,
          NumberOfContacts = r.NumberOfContacts == null
            ? 0
            : r.NumberOfContacts.Value,
          ProgrammeOutcome = r.ProgrammeOutcome,
          ProviderName = r.Provider == null
            ? null
            : r.Provider.Name,
          ProviderSubmissions = r.ProviderSubmissions
            .Select(ps => new ProviderSubmission()
            {
              Coaching = ps.Coaching,
              Date = ps.Date,
              Measure = ps.Measure,
              SubmissionDate = ps.ModifiedAt,
              Weight = ps.Weight
            })
            .ToList(),
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
          HasHadBariatricSurgery = r.HasHadBariatricSurgery
        })        
        .ToList();

      return anonymisedReferralList;
    }

    /// <summary>
    /// Returns a IEnumerable of active NhsNumberTrace objects consisting of 
    /// referrals whose NHS number property is null
    /// NHS Number traceing is continual depending on ReferralStatus or max
    /// 2 years after completion.
    /// Linq extensions and ReferralStatus extensions use attributes to set
    /// the restrictions on the list returned
    /// </summary>
    /// <returns>IEnumerable of NhsNumberTrace objects</returns>
    public async Task<IEnumerable<NhsNumberTrace>> GetUntracedNhsNumbers()
    {
      List<NhsNumberTraceReferral> query = await _context.Referrals
        .AsNoTracking()
        .Where(r => r.IsActive)
        .Where(r => r.NhsNumber == null)
        .Where(r => r.DateOfBirth != null)
        .Where(r => r.FamilyName != null)
        .Where(r => r.GivenName != null)
        .Where(r => r.Postcode != null)
        .WhereDateComparesAsync(730);


      IEnumerable<NhsNumberTrace> untracedNhsNumbers = query.Where(
          t => t.LastTraceDate == null ||
               t.LastTraceDate.IsTraceDateValid<ReferralStatus>(t.Status))
        .Where(t => t.Status.CanTrace<ReferralStatus>())
        .Select(r => new NhsNumberTrace
        {
          DateOfBirth = r.DateOfBirth,
          FamilyName = r.FamilyName,
          GivenName = r.GivenName,
          Id = r.Id,
          Postcode = r.Postcode
        })
        .ToList();

      return untracedNhsNumbers;
    }

    /// <summary>
    /// Updates the TraceCount, LastTraceDate, ModifiedAt and ModifiedByUserId
    /// properties of each referal matched on the Id of each of the 
    /// SpineTraceResult objects.
    /// If the trace was successful: NhsNumber, ReferringGpPracticeNumber 
    /// and ReferringGpPracticeName propeties are updated to their traced 
    /// values. If the referal is a self referral and the traced NHS number
    /// is already associated with another referral then the referral's status
    /// will be set to CancelledDuplicateTextMessage. If the referral's status
    /// is ProviderAwaitingTrace then it will be updated to 
    /// ProviderAwaitingStart.
    /// If the trace was unsuccessful: ReferringGpPracticeNumber 
    /// and ReferringGpPracticeName propeties are updated their unknown 
    /// constants.</summary>
    /// <param name="spineTraceResults">An IEnumerable of SpineTraceResult 
    /// objects.</param>
    /// <exception cref="ArgumentNullException">Thrown when the 
    /// SpineTraceResults parameter is null.</exception>
    /// <exception cref="ValidationException">Thrown when one of the
    /// spineTraceResults model propeties fails a validation check.</exception> 
    /// <exception cref="ReferralNotFoundException">Thrown when the Id of one
    /// of the SpineTraceResult objects does not match an existing referral.
    /// </exception>
    /// <exception cref="NhsNumberTraceMismatchException">Thrown when the NHS
    /// number of the existing referral is not null or does not match the 
    /// the traced value.</exception>
    public async Task UpdateSpineTraced(
      IEnumerable<SpineTraceResult> spineTraceResults)
    {
      if (spineTraceResults is null)
      {
        throw new ArgumentNullException(nameof(spineTraceResults));
      }

      foreach (SpineTraceResult traceResult in spineTraceResults)
      {
        ValidateModelResult validationResult = ValidateModel(traceResult);
        if (!validationResult.IsValid)
        {
          throw new ValidationException(validationResult.GetErrorMessage());
        }

        Referral referral = await _context.Referrals.FindAsync(traceResult.Id);
        if (referral == null)
        {
          throw new ReferralNotFoundException(traceResult.Id);
        }

        if (referral.NhsNumber != null &&
          referral.NhsNumber != traceResult.NhsNumber)
        {
          throw new NhsNumberTraceMismatchException(
            referral.NhsNumber, traceResult.NhsNumber);
        }

        referral.LastTraceDate = DateTimeOffset.Now;
        referral.TraceCount = referral.TraceCount == null
          ? 1
          : referral.TraceCount + 1;

        if (traceResult.IsTraceSuccessful)
        {
          referral.NhsNumber = traceResult.NhsNumber;
          referral.ReferringGpPracticeNumber = traceResult.GpPracticeOdsCode;
          referral.ReferringGpPracticeName = traceResult.GpPracticeName;

          // only care about duplicates for self-referrals
          if (referral.ReferralSource == ReferralSource.SelfReferral.ToString())
          {
            Referral duplicate = await _context.Referrals
              .Where(r => r.IsActive)
              .Where(r => r.Id != referral.Id)
              .FirstOrDefaultAsync(r => r.NhsNumber == referral.NhsNumber);

            if (duplicate != null)
            {
              referral.Status =
                ReferralStatus.CancelledDuplicateTextMessage.ToString();
              referral.StatusReason =
                $"Duplicate NHS number found in UBRN {duplicate.Ubrn}.";
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
          referral.StatusReason = null;
        }

        UpdateModified(referral);
      }

      await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ReprocessedReferral>>
      GetAnonymisedReprocessedReferralsBySubmissionDate(
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null)
    {

      List<ReprocessedReferral> model = _context.ReferralsAudit
        .AsNoTracking()
        .Where(r => r.IsActive)
        .Where(r => r.DateOfReferral == null || 
                    (r.DateOfReferral >= fromDate && 
                     r.DateOfReferral <= toDate))
        .Where(s => s.ReferralSource == ReferralSource.GpReferral.ToString())
        .Where(t => _context.ReferralsAudit.OrderBy(s => s.ModifiedAt)
          .FirstOrDefault(
            s => (s.Status == ReferralStatus.New.ToString() ||
                  s.Status == ReferralStatus.Exception.ToString() ||
                  s.Status == ReferralStatus.RmcCall.ToString())
                 && s.Ubrn == t.Ubrn) != null)
        .Select(t => new
        {
          t.Ubrn,
          t.ModifiedAt,
          t.ReferringGpPracticeNumber,
          t.StatusReason,
          t.DateOfReferral,
          t.Status
        }).ToList()
        .GroupBy(t => new { t.Ubrn }, (key, g) => g.OrderBy(r => r.ModifiedAt))
        .Select(rg => new ReprocessedReferral
        {
          DateOfReferral = rg.First().DateOfReferral,
          StatusArray = rg.Select(t => t.Status).ToArray(),
          Ubrn = rg.First().Ubrn,
          InitialStatusReason = rg.First().StatusReason,
          CurrentlyCancelledStatusReason = rg.Last().StatusReason,
          ReferringGpPracticeCode = rg.First().ReferringGpPracticeNumber
        }).ToList();


      return model;
    }

    public async Task<IEnumerable<AnonymisedReferral>>
  GetAnonymisedReferralsForUbrn(string ubrn)
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
        .Select(r => new AnonymisedReferralHistory()
        {
          Age = r.DateOfBirth == null
            ? null
            : r.DateOfBirth.Value.GetAge(),
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
            : ((MethodOfContact)(r.MethodOfContact.Value)).ToString(),
          ModifiedByUserId = r.ModifiedByUserId,
          NumberOfContacts = r.NumberOfContacts == null
            ? 0
            : r.NumberOfContacts.Value,
          ProgrammeOutcome = r.ProgrammeOutcome,
          ProviderName = r.Provider == null
            ? null
            : r.Provider.Name,
          ProviderSubmissions = r.ProviderSubmissions
            .Select(ps => new ProviderSubmission()
            {
              Coaching = ps.Coaching,
              Date = ps.Date,
              Measure = ps.Measure,
              SubmissionDate = ps.ModifiedAt,
              Weight = ps.Weight
            })
            .ToList(),
          ReferralSource = r.ReferralSource,
          ReferringGpPracticeNumber = r.ReferringGpPracticeNumber,
          Sex = r.Sex.TryParseSex(),
          Status = r.Status,
          StatusReason = r.StatusReason,
          TriagedCompletionLevel =
            string.IsNullOrWhiteSpace(r.TriagedCompletionLevel)
              ? null
              : int.Parse(r.TriagedCompletionLevel),
          Ubrn = r.Ubrn,
          VulnerableDescription = r.VulnerableDescription.TryParseToAnonymous(),
          StaffRole = r.ReferralSource == ReferralSource.SelfReferral.ToString()
            ? r.StaffRole
            : null,
          ConsentForGpAndNhsNumberLookup = r.ConsentForGpAndNhsNumberLookup,
          ConsentForReferrerUpdatedWithOutcome =
            r.ConsentForReferrerUpdatedWithOutcome,
          ReferringOrganisationOdsCode = r.ReferringOrganisationOdsCode
        })
        .ToListAsync();

      foreach (var item in list)
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
  }
}


