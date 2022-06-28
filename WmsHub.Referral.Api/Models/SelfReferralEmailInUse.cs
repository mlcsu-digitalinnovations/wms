using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;

namespace WmsHub.Referral.Api.Models
{
  public class SelfReferralEmailInUse
  {
    private string _email;

    [Required, NhsEmail]
    [StringLength(200)]

    public string Email
    {
      get => _email;
      set => _email = value?.Trim().ToLower();
    }
  }
}
