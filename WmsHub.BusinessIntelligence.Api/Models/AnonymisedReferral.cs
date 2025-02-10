using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using WmsHub.Business.Models;
using WmsHub.BusinessIntelligence.Api.SwaggerSchema;

namespace WmsHub.BusinessIntelligence.Api.Models;

[SwaggerSchemaFilter(typeof(AnonymisedReferralSchemaFilter))]
public class AnonymisedReferral
{
  [SwaggerSchema("Current status of the referral.")]
  public string Status { get; set; }
  [SwaggerSchema("Reason for the last status change.")]
  public string StatusReason { get; set; }
  [SwaggerSchema("Referring GP Practice ODS Code.")]
  public string ReferringGpPracticeNumber { get; set; }
  [SwaggerSchema("If referral is from GP then eRS UBRN else system " +
    "generated.")]
  public string Ubrn { get; set; }
  [SwaggerSchema("Index of Multiple Deprivation for the referral postcode.")]
  public string Deprivation { get; set; }
  [SwaggerSchema("Age of the service user at time of referral.")]
  public int Age { get; set; }
  [SwaggerSchema("Description of the last contact method.")]
  public string MethodOfContact { get; set; }
  [SwaggerSchema("Number of times the service user has been contacted.")]
  public int NumberOfContacts { get; set; }
  [SwaggerSchema("Service user's sex at birth.")]
  public string Sex { get; set; }
  [SwaggerSchema("Date of referral.", Format = "DateTime", Nullable = false)]
  public DateTimeOffset DateOfReferral { get; set; }
  [SwaggerSchema("Future contact consent.", Nullable = false)]
  public bool ConsentForFutureContactForEvaluation { get; set; }
  public string Ethnicity { get; set; }
  [SwaggerSchema("Service user has hypertension.")]
  public bool HasHypertension { get; set; }
  [SwaggerSchema("Service user has type 1 diabetes.")]
  public bool HasDiabetesType1 { get; set; }
  [SwaggerSchema("Service user has type 2 diabetes.")]
  public bool HasDiabetesType2 { get; set; }
  [SwaggerSchema("Service user's height in centimeters.")]
  public decimal HeightCm { get; set; }
  [SwaggerSchema("Service user's weight in kilograms.")]
  public decimal GpRecordedWeight { get; set; }
  [SwaggerSchema("Service user's initial BMI.")]
  public decimal CalculatedBmiAtRegistration { get; set; }
  [SwaggerSchema("Is the service user vulnerable?")]
  public bool IsVulnerable { get; set; }
  [SwaggerSchema("Does the service user had a registered serious " +
    "mental illness?")]
  public bool HasRegisteredSeriousMentalIllness { get; set; }
  [SwaggerSchema("Service user triage level.")]
  public int? TriagedCompletionLevel { get; set; }
  [SwaggerSchema("Service provider name.")]
  public string ProviderName { get; set; }
  [SwaggerSchema(
    "Date service user completed the Weight Management Programme.",
    Format = "Date")]
  public DateTimeOffset? DateCompletedProgramme { get; set; }
  [SwaggerSchema("Date of service user's initial BMI.", Format = "Date")]
  public DateTimeOffset DateOfBmiAtRegistration { get; set; }
  [SwaggerSchema("Date service user selected a provider.", Format = "Date")]
  public DateTimeOffset? DateOfProviderSelection { get; set; }
  [SwaggerSchema("Date service user started with provider.", Format = "Date")]
  public DateTimeOffset? DateStartedProgramme { get; set; }
  [SwaggerSchema("Date referral will be re-added to the RMC call list.",
    Format = "Date")]
  public DateTimeOffset? DateToDelayUntil { get; set; }
  [SwaggerSchema("Outcome of the provider programme.")]
  public string ProgrammeOutcome { get; set; }
  public List<ProviderSubmission> ProviderSubmissions { get; set; }
  [SwaggerSchema("Description of service user's vulnerability.")]
  public string VulnerableDescription { get; set; }
  [SwaggerSchema("Service user has physical disability.")]
  public bool HasAPhysicalDisability { get; set; }
  [SwaggerSchema("Service user has a learning disability.")]
  public bool HasALearningDisability { get; set; }
  [SwaggerSchema("Date provider first contacted the service user.",
    Format = "Date",
    Nullable = true)]
  public DateTimeOffset? DateOfProviderContactedServiceUser { get; set; }
  [SwaggerSchema("Referral pathway.")]
  public string ReferralSource { get; set; }
  [SwaggerSchema("Staff referral service user's role.")]
  public string StaffRole { get; set; }
  [SwaggerSchema("ODS code of the referring organisation.")]
  public string ReferringOrganisationOdsCode { get; set; }
  [SwaggerSchema("The service user has arthritis in the knee.")]
  public bool? HasArthritisOfKnee { get; set; }
  [SwaggerSchema("The service user has arthritis in the hip.")]
  public bool? HasArthritisOfHip { get; set; }
  [SwaggerSchema("The service user is pregnant.")]
  public bool? IsPregnant { get; set; }
  [SwaggerSchema("The service user has an active eating disorder.")]
  public bool? HasActiveEatingDisorder { get; set; }
  [SwaggerSchema("The service user has undergone barbaric surgery.")]
  public bool? HasHadBariatricSurgery { get; set; }
  [SwaggerSchema("The service user has given consent to lookup GP ODS " +
    "code and NHS number.")]
  public bool? ConsentForGpAndNhsNumberLookup { get; set; }
  [SwaggerSchema("The service user given consent to update the referring " +
    "organisation with their programme outcome.")]
  public bool? ConsentForReferrerUpdatedWithOutcome { get; set; }
  [SwaggerSchema("The triage level offered to the service user.")]
  public string OfferedCompletionLevel { get; set; }
  [SwaggerSchema("The version of the referral letter received from eRS.")]
  public decimal? DocumentVersion { get; set; }
  [SwaggerSchema("The id of the eRS service.")]
  public string ServiceId { get; set; }
  [SwaggerSchema("The name of the GP's system.")]
  public string SourceSystem { get; set; }
  [SwaggerSchema("The unique id shared with the provider.")]
  public string ProviderUbrn { get; set; }
  [SwaggerSchema("Service user's selected ethnicity in the service user UI.")]
  public string ServiceUserEthnicity { get; set; }
  [SwaggerSchema("Service user's selected ethnicity group in the service " +
    "user UI.")]
  public string ServiceUserEthnicityGroup { get; set; }
  public DateTimeOffset? ReferralLetterDate { get; set; }
  [SwaggerSchema("OPCS code(s) for service user's forthcoming surgical" +
    "procedure(s)")]
  public string OpcsCodes { get; set; }
  [SwaggerSchema("The date service user was placed on their current waiting " +
    "list for surgery.")]
  public DateTimeOffset? DatePlacedOnWaitingList { get; set; }
}

