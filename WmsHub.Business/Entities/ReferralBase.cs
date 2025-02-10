using System;
using System.ComponentModel.DataAnnotations.Schema;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Entities;

// Additional config in OnModelCreating
public abstract class ReferralBase : BaseEntity
{
  // Minimum Data Set
  public string NhsNumber { get; set; }
  public DateTimeOffset? DateOfReferral { get; set; }
  public string ReferringGpPracticeNumber { get; set; }
  public string Ubrn { get; set; }
  public string FamilyName { get; set; }
  public string GivenName { get; set; }
  public string Address1 { get; set; }
  public string Address2 { get; set; }
  public string Address3 { get; set; }
  public string Postcode { get; set; }
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
  [Column(TypeName = "decimal(18,2)")]
  public decimal? HeightCm { get; set; }
  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightKg { get; set; }
  [Column(TypeName = "decimal(18,2)")]
  public decimal? CalculatedBmiAtRegistration { get; set; }
  public DateTimeOffset? DateOfBmiAtRegistration { get; set; }
  public string TriagedCompletionLevel { get; set; }
  public DateTimeOffset? DateOfProviderSelection { get; set; }
  public DateTimeOffset? DateStartedProgramme { get; set; }
  public DateTimeOffset? DateCompletedProgramme { get; set; }
  public DateTimeOffset? DateOfProviderContactedServiceUser { get; set; }
  public string ProgrammeOutcome { get; set; }

  // Additional Properties    
  public Guid? ProviderId { get; set; }
  public string ReferringGpPracticeName { get; set; }
  public string Status { get; set; }
  public string StatusReason { get; set; }
  public string TriagedWeightedLevel { get; set; }

  public DateTimeOffset? DateToDelayUntil { get; set; }
  public string DelayReason { get; set; }
  public bool? IsTelephoneValid { get; set; }
  public bool? IsMobileValid { get; set; }

  public string ReferralAttachmentId { get; set; }

  public DateTimeOffset? MostRecentAttachmentDate { get; set; }

  public string Deprivation { get; set; }

  public string ServiceUserEthnicity { get; set; }
  public string ServiceUserEthnicityGroup { get; set; }
  public DateTimeOffset? DateLetterSent { get; set; }
  public int? MethodOfContact { get; set; }
  public int NumberOfContacts { get; set; }

  public string StaffRole { get; set; }
  public string ReferralSource { get; set; }
  public string ReferringOrganisationOdsCode { get; set; }
  public bool? ConsentForGpAndNhsNumberLookup { get; set; }
  public bool? ConsentForReferrerUpdatedWithOutcome { get; set; }
  public string ReferringOrganisationEmail { get; set; }
  public DateTimeOffset? CreatedDate { get; set; }

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
  [Column(TypeName = "decimal(18,2)")]
  public decimal? DocumentVersion { get; set; }
  public Common.Enums.SourceSystem? SourceSystem { get; set; }
  [Column(TypeName = "decimal(18,2)")]
  public decimal? FirstRecordedWeight { get; set; }
  public DateTimeOffset? FirstRecordedWeightDate { get; set; }
  [Column(TypeName = "decimal(18,2)")]
  public decimal? LastRecordedWeight { get; set; }
  public DateTimeOffset? LastRecordedWeightDate { get; set; }

  public string ReferringClinicianEmail { get; set; }
  public string CreatedByUserId { get; set; }

  public string ProviderUbrn { get; set; }

  // Elective Care
  public DateTimeOffset? DatePlacedOnWaitingList { get; set; }
  public string OpcsCodes { get; set; }
  public string SourceEthnicity { get; set; }
  public bool? SurgeryInLessThanEighteenWeeks { get; set; }
  public int? WeeksOnWaitingList { get; set; }
  public string SpellIdentifier { get; set; }

  public DateTimeOffset? ReferralLetterDate { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? HeightFeet { get; set; }
  [Column(TypeName = "decimal(18,2)")]
  public decimal? HeightInches { get; set; }
  public UnitsType? HeightUnits { get; set; } = UnitsType.Metric;
  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightStones { get; set; }
  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightPounds { get; set; }  
  public UnitsType? WeightUnits { get; set; } = UnitsType.Metric;
  public bool? IsErsClosed { get; set; }
}
