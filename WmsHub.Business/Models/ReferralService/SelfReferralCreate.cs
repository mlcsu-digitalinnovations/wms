using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;

namespace WmsHub.Business.Models.ReferralService
{
  public class SelfReferralCreate : AReferralCreate,
    ISelfReferralCreate, IValidatableObject
  {
    [Required, NhsEmail]
    [MaxLength(200)]
    public override string Email { get; set; }

    [Required]
    public string StaffRole { get; set; }

    [Required]
    public bool? ConsentForFutureContactForEvaluation { get; set; }
  }
}
