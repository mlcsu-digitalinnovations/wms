using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;

namespace WmsHub.Referral.Api.Models
{
  public abstract class APharmacy
  {
    [Required]
    [NhsEmail]
    [MaxLength(200)]
    public virtual string Email { get; set; }
    [Required, PharmacyOdsCode]
    [MaxLength(450)]
    public virtual string OdsCode { get; set; }
    [Required]
    public virtual string TemplateVersion { get; set; }

  }
}