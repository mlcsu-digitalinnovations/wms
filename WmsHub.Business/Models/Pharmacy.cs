using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;

namespace WmsHub.Business.Models
{
  public class Pharmacy : BaseModel, IPharmacy
  {
    [NhsEmail]
    public string Email { get; set; }
    [Required, PharmacyOdsCode]
    public string OdsCode { get; set; }
    [Required]
    public string TemplateVersion { get; set; }

  }
}