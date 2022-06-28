using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models.ReferralService.MskReferral
{
  public interface IMskReferralCreate : IAReferralCreate
  {
    bool? ConsentForReferrerUpdatedWithOutcome { get; set; }
    string CreatedByUserId { get; set; }
    bool? HasActiveEatingDisorder { get; set; }
    bool? HasArthritisOfHip { get; set; }
    bool? HasArthritisOfKnee { get; set; }
    bool? HasHadBariatricSurgery { get; set; }
    bool? IsPregnant { get; set; }
    string NhsNumber { get; set; }
    string ReferringGpPracticeNumber { get; set; }
    string ReferringMskClinicianEmailAddress { get; set; }
    string ReferringMskHubOdsCode { get; set; }

    IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
  }
}