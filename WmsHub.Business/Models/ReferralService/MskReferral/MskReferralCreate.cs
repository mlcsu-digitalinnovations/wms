using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Common.Attributes;

namespace WmsHub.Business.Models.ReferralService.MskReferral
{
  public class MskReferralCreate : 
    AReferralCreate, IMskReferralCreate, IValidatableObject
  {
    public MskReferralCreate()
    {
      DateTimeOffset now = DateTimeOffset.Now;
      IsActive = true;
      CreatedDate = now;
      DateOfReferral = now;
      ReferralSource = Enums.ReferralSource.Msk.ToString();
      Status = Enums.ReferralStatus.New.ToString();
    }
    
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedDate { get; private set; }
    public DateTimeOffset DateOfReferral { get; private set; }
    public string ReferralSource { get; private set; }
    public string Status { get; private set; }

    [MustBeTrue]
    public bool ConsentForGpAndNhsNumberLookup { get; set; }
    [Required]
    [NhsNumber]
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
    public string ReferringGpPracticeNumber { get; set; }
    [Required]
    public string ReferringMskHubOdsCode { get; set; }
    [Required]
    public string ReferringMskClinicianEmailAddress { get; set; }
    [Required]
    public string CreatedByUserId { get; set; }
    [Required]
    public bool? ConsentForReferrerUpdatedWithOutcome { get; set; }

    public override IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      IEnumerable<ValidationResult> baseValidationResults = base.Validate(validationContext);

      if (baseValidationResults.Any())
      {
        foreach (ValidationResult result in baseValidationResults)
        {
          yield return result;
        }
      }

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