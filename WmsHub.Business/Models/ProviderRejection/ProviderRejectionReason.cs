using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;

namespace WmsHub.Business.Models
{
  public class ProviderRejectionReason : BaseModel, IProviderRejectionReason
  {
    [Required, MaxLength(100), StringNoSpace]
    public string Title { get; set; }
    [Required, MaxLength(500)]
    public string Description { get; set; }
  }
}
