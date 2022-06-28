using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Models
{
  public class Referral : BaseModel, IReferral
  {
    public DateTimeOffset? CreatedDate { get; set; }
    [NhsNumber] public string NhsNumber { get; set; }

    [CsvExport(Order = 9, Format = "dd/MM/yyyy")]
    public DateTimeOffset? DateOfReferral { get; set; }

    public string ReferringGpPracticeNumber { get; set; }
    [CsvExport(Order = 1)] public string Ubrn { get; set; }
    [CsvExport(Order = 2)] public string FamilyName { get; set; }
    [CsvExport(Order = 3)] public string GivenName { get; set; }
    [CsvExport(Order = 4)] public string Address1 { get; set; }
    [CsvExport(Order = 5)] public string Address2 { get; set; }
    [CsvExport(Order = 6)] public string Address3 { get; set; }
    [CsvExport(Order = 7)] public string Postcode { get; set; }
    public string Telephone { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string Sex { get; set; }
    public bool? IsVulnerable { get; set; }
    public string VulnerableDescription { get; set; }
    public bool? ConsentForFutureContactForEvaluation { get; set; }
    public string Ethnicity { get; set; }
    public bool? HasAPhysicalDisability { get; set; }
    public bool? HasALearningDisability { get; set; }
    public bool? HasRegisteredSeriousMentalIllness { get; set; }
    public bool? HasHypertension { get; set; }
    public bool? HasDiabetesType1 { get; set; }
    public bool? HasDiabetesType2 { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public string CriDocument { get; set; }
    public DateTimeOffset? CriLastUpdated { get; set; }
    public decimal? CalculatedBmiAtRegistration { get; set; }
    public DateTimeOffset? DateOfBmiAtRegistration { get; set; }
    public bool IsBmiTooLow { get; set; }
    public decimal SelectedEthnicGroupMinimumBmi { get; set; }
    public string TriagedCompletionLevel { get; set; }
    public DateTimeOffset? DateOfProviderSelection { get; set; }
    public DateTimeOffset? DateStartedProgramme { get; set; }
    public DateTimeOffset? DateCompletedProgramme { get; set; }
    public DateTimeOffset? DateOfProviderContactedServiceUser { get; set; }

    [CsvExport(Order = 8, Format = "dd/MM/yyyy")]
    public DateTimeOffset? DateLetterSent { get; set; }

    public string ProgrammeOutcome { get; set; }

    // Additional Properties
    public string ReferringGpPracticeName { get; set; }
    public string Status { get; set; }
    public string StatusReason { get; set; }
    public string TriagedWeightedLevel { get; set; }
    public bool? IsTelephoneValid { get; set; }
    public bool? IsMobileValid { get; set; }
    public DateTimeOffset? DelayUntil { get; set; }
    public string DelayReason { get; set; }
    public long? ReferralAttachmentId { get; set; }
    public long? MostRecentAttachmentId { get; set; }
    public string Deprivation { get; set; }
    public string StaffRole { get; set; }

    public List<Call> Calls { get; set; }
    public List<ProviderSubmission> ProviderSubmissions { get; set; }
    public List<TextMessage> TextMessages { get; set; }
    public List<Provider> Providers { get; set; }
    public Guid? ProviderId { get; set; }
    public string ServiceUserEthnicity { get; set; }
    public string ServiceUserEthnicityGroup { get; set; }
    public Provider Provider { get; set; }
    public int? MethodOfContact { get; set; }
    public int? NumberOfContacts { get; set; }

    public string ReferralSource { get; set; }
    public string ReferringOrganisationOdsCode { get; set; }
    public bool? ConsentForGpAndNhsNumberLookup { get; set; }
    public bool? ConsentForReferrerUpdatedWithOutcome { get; set; }
    public string ReferringOrganisationEmail { get; set; }

    public MethodOfContact ContactMethod =>
      MethodOfContact == null
        ? Enums.MethodOfContact.NoContact
        : (MethodOfContact)MethodOfContact;

    public string GetValidNumber()
    {
      return IsMobileValid ?? true
        ? Mobile
        : IsTelephoneValid ?? true
          ? Telephone
          : null;
    }

    public bool HasProviders => Providers?.Count > 0;

    public bool HasValidNumber
    {
      get { return (IsMobileValid ?? true) || (IsTelephoneValid ?? true); }
    }

    public bool IsGpReferral =>
      ReferralSource == Enums.ReferralSource.GpReferral.ToString();

    public bool IsProviderSelected => ProviderId != null;

    public bool IsExceptionDueToEmailNotProvided => (TextMessages == null ||
      TextMessages.Any(t => t.HasDoNotContactOutcome));

    public DateTimeOffset? LastTraceDate { get; set; }
    public int? TraceCount { get; set; }
    public bool? HasArthritisOfKnee { get; set; }
    public bool? HasArthritisOfHip { get; set; }
    public bool? IsPregnant { get; set; }
    public bool? HasActiveEatingDisorder { get; set; }
    public bool? HasHadBariatricSurgery { get; set; }

    public string NhsLoginClaimFamilyName { get; set; }
    public string NhsLoginClaimGivenName { get; set; }
    public string NhsLoginClaimMobile { get; set; }
    public virtual string NhsLoginClaimEmail { get; set; }

    public string OfferedCompletionLevel { get; set; }
    public string ServiceId { get; set; }
    public decimal? DocumentVersion { get; set; }
    public Common.Enums.SourceSystem? SourceSystem { get; set; }
    public decimal? FirstRecordedWeight { get; set; }
    public DateTimeOffset? FirstRecordedWeightDate { get; set; }
    public decimal? LastRecordedWeight { get; set; }
    public DateTimeOffset? LastRecordedWeightDate { get; set; }
    public int NumberOfDelays { get; set; }
    public string ReferringClinicianEmail { get; set; }
    public string CreatedByUserId { get; set; }

    internal Entities.Call CreateNewChatBotCall(ClaimsPrincipal user)
    {
      if (user is null)
        throw new ArgumentNullException(nameof(user));

      return new Entities.Call
      {
        IsActive = true,
        ModifiedAt = DateTimeOffset.Now,
        ModifiedByUserId = user.GetUserId(),
        Number = GetValidNumber(),
        ReferralId = Id
      };
    }
  }
}
