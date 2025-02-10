using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models.ReferralService;

public class SelfReferralCreate 
  : AReferralCreate, ISelfReferralCreate, IValidatableObject
{
  [Required]
  [EmailAddress]
  [MaxLength(200)]
  public override string Email { get; set; }

  [Required]
  public string StaffRole { get; set; }

  [Required]
  public bool? ConsentForFutureContactForEvaluation { get; set; }
}
