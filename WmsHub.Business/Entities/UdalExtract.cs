using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WmsHub.Business.Entities;

public class UdalExtract
{
  [Key]
  public Guid ReferralId { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? CalculatedBmiAtRegistration { get; set; }

  public bool? Coaching0007 { get; set; }
  
  public bool? Coaching0814 { get; set; }
  
  public bool? Coaching1521 { get; set; }
  
  public bool? Coaching2228 { get; set; }
  
  public bool? Coaching2935 { get; set; }
  
  public bool? Coaching3642 { get; set; }
  
  public bool? Coaching4349 { get; set; }
  
  public bool? Coaching5056 { get; set; }
  
  public bool? Coaching5763 { get; set; }
  
  public bool? Coaching6470 { get; set; }
  
  public bool? Coaching7177 { get; set; }
  
  public bool? Coaching7884 { get; set; }

  public bool? ConsentForFutureContactForEvaluation { get; set; }

  public DateTime? DateCompletedProgramme { get; set; }

  public DateTimeOffset? DateOfBirth { get; set; }

  public DateTime? DateOfBmiAtRegistration { get; set; }

  public DateTime? DateOfProviderContactedServiceUser { get; set; }

  public DateTime? DateOfProviderSelection { get; set; }

  public DateTime? DateOfReferral { get; set; }

  public DateTime? DatePlacedOnWaitingListForElectiveCare { get; set; }

  public DateTime? DateStartedProgramme { get; set; }

  public DateTime? DateToDelayUntil { get; set; }

  [StringLength(200)]
  public string DeprivationQuintile { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? DocumentVersion { get; set; }

  [StringLength(200)]
  public string Ethnicity { get; set; }

  [StringLength(200)]
  public string EthnicityGroup { get; set; }

  [StringLength(200)]
  public string EthnicitySubGroup { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? GpRecordedWeight { get; set; }

  [StringLength(200)]
  public string GpSourceSystem { get; set; }

  public bool? HasALearningDisability { get; set; }

  public bool? HasAPhysicalDisability { get; set; }

  public bool? HasDiabetesType1 { get; set; }

  public bool? HasDiabetesType2 { get; set; }

  public bool? HasHypertension { get; set; }

  public bool? HasRegisteredSeriousMentalIllness { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? HeightCm { get; set; }

  public bool? IsVulnerable { get; set; }

  [StringLength(200)]
  public string MethodOfContact { get; set; }

  public DateTime? ModifiedAt { get; set; }

  public string NhsNumber { get; set; }

  public int? NumberOfContacts { get; set; }

  [StringLength(200)]
  public string OpcsCodesForElectiveCare { get; set; }

  public bool? ProviderEngagement0007 { get; set; }
  
  public bool? ProviderEngagement0814 { get; set; }
  
  public bool? ProviderEngagement1521 { get; set; }
  
  public bool? ProviderEngagement2228 { get; set; }
  
  public bool? ProviderEngagement2935 { get; set; }
  
  public bool? ProviderEngagement3642 { get; set; }
  
  public bool? ProviderEngagement4349 { get; set; }
  
  public bool? ProviderEngagement5056 { get; set; }
  
  public bool? ProviderEngagement5763 { get; set; }
  
  public bool? ProviderEngagement6470 { get; set; }
  
  public bool? ProviderEngagement7177 { get; set; }
  
  public bool? ProviderEngagement7884 { get; set; }

  [StringLength(200)]
  public string ProviderName { get; set; }

  [StringLength(200)]
  public string ProviderUbrn { get; set; }

  [StringLength(200)]
  public string ReferralSource { get; set; }

  [StringLength(200)]
  public string ReferringGpPracticeNumber { get; set; }

  [StringLength(200)]
  public string ReferringOrganisationOdsCode { get; set; }

  [StringLength(200)]
  public string ServiceId { get; set; }

  [StringLength(200)]
  public string Sex { get; set; }

  [StringLength(200)]
  public string StaffRole { get; set; }

  [StringLength(200)]
  public string Status { get; set; }

  public int? TriagedCompletionLevel { get; set; }

  public UdalExtractsHistory UdalExtractHistory { get; set; }

  public int UdalExtractHistoryId { get; set; }

  public string VulnerableDescription { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement0007 { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement0814 { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement1521 { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement2228 { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement2935 { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement3642 { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement4349 { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement5056 { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement5763 { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement6470 { get; set; }
  
  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement7177 { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement7884 { get; set; }

  [Column(TypeName = "decimal(18,2)")]
  public decimal? WeightMeasurement8500 { get; set; }
}
