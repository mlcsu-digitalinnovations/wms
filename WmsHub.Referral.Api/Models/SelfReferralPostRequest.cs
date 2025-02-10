using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models;

public class SelfReferralPostRequest : AReferralPostRequest,
  IValidatableObject
{
  private string _email;

  [Required]
  public bool? ConsentForFutureContactForEvaluation { get; set; }

  [Required]
  [EmailAddress]
  [MaxLength(200)]
  public override string Email
  {
    get => _email;
    set => _email = value?.Trim().ToLower();
  }

  [Required]
  public string StaffRole { get; set; }
}

