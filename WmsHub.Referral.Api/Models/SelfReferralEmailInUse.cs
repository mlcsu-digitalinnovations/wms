using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models;

public class SelfReferralEmailInUse
{
  private string _email;

  [Required]
  [EmailAddress]
  [StringLength(200)]
  public string Email
  {
    get => _email;
    set => _email = value?.Trim().ToLower();
  }
}
