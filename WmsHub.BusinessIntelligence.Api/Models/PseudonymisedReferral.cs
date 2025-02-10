using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace WmsHub.BusinessIntelligence.Api.Models;

public class PseudonymisedReferral : AnonymisedReferral
{
  [SwaggerSchema("Service user has received coaching.")]
  public bool HasReceivedCoaching { get; set; }
  [SwaggerSchema("Referral is eligible.")]
  public bool IsEligible { get; set; }
  [SwaggerSchema("NHS Number.")]
  public string NhsNumber { get; set; }
  [SwaggerSchema("OPCS code(s) for service user's forthcoming surgical procedure(s)")]
  [JsonPropertyName("OPCSCodesForElectiveCare")]
  public string OpcsCodesForElectiveCare { get; set; }
  [SwaggerSchema("Referral pathway.")]
  public string ReferralSourceDescription { get; set; }
  [SwaggerSchema("Service user triage level.")]
  public string TriagedCompletionLevelString { get; set; }
  [SwaggerSchema("Service user's selected ethnicity in the service user UI.")]
  public string ServiceUserEthnicitySubgroup { get; set; }
}

