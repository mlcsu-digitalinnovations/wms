using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;
using WmsHub.Common.Helpers;

namespace WmsHub.Referral.Api.Models.MskReferral
{
  public class PostRequest : AReferralPostRequest, IValidatableObject
  {
    [Required]
    [NhsNumber(allowNulls:false)]
    public string NhsNumber { get; set; }
    public bool? HasArthritisOfKnee { get; set; }
    public bool? HasArthritisOfHip { get; set; }
    [NotTrue]
    public bool? IsPregnant { get; set; }
    [NotTrue]
    public bool? HasActiveEatingDisorder { get; set; }
    [NotTrue]
    public bool? HasHadBariatricSurgery { get; set; }
    [Required]
    [GpPracticeOdsCode]
    public string ReferringGpPracticeNumber { get; set; }
    [Required]
    public string ReferringMskHubOdsCode { get; set; }
    [Required]
    [RegularExpression(
      Constants.REGEX_EMAIL_ADDRESS,
      ErrorMessage = $"The {nameof(ReferringMskClinicianEmailAddress)} " +
        $"field is invalid.")]
    public string ReferringMskClinicianEmailAddress { get; set; }
    [Required]
    public string CreatedByUserId { get; set; }
    [Required]
    public bool? ConsentForReferrerUpdatedWithOutcome { get; set; }
    [MustBeTrue]
    public bool? ConsentForGpAndNhsNumberLookup { get; set; }

    public override IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      base.Validate(validationContext);

      if ((HasArthritisOfHip ?? false) == false &&
        (HasArthritisOfKnee ?? false) == false)
      {
        yield return new ValidationResult(
          $"Either {nameof(HasArthritisOfHip)} or " +
            $"{nameof(HasArthritisOfKnee)} must be true.",
          new string[] 
          { 
            nameof(HasArthritisOfHip),
            nameof(HasArthritisOfKnee)
          });
      }
    }
  }
}
