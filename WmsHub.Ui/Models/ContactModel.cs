using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Ui.Validations;

namespace WmsHub.Ui.Models
{
  public class ContactModel : BaseModel
  {
    [Required(ErrorMessage = "Select yes to take part in future surveys")]
    public bool CanContact { get; set; }
    [EmailValidationWhenConsented]
    public string Email { get; set; }
    public bool DontContactByEmail { get; set; }
    public bool IsConfirmingEmail => !string.IsNullOrWhiteSpace(Email);
    public ReferralSource Source { get; set; }
    public bool HasGpReferralSource => Source == ReferralSource.GpReferral;
  }
}