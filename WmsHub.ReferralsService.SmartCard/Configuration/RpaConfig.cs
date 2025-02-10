using System.ComponentModel.DataAnnotations;
using WmsHub.ReferralsService.SmartCard.Models;

namespace WmsHub.ReferralsService.SmartCard.Configuration
{
  public class RpaConfig
  {
    [Required]
    public float NumberOfConnectionAttempts { get; set; }
    [Required]
    public string IsosecIoIdentityAgentName { get; set; }
    [Required]
    public string SmartCardEmailAddress { get; set; }
    [Required]
    public string SmartCardPassword { get; set; }
    [Required]
    public float TimeDelayMultiplier { get; set; }    
    public DialogDetails BlockingDialogServerUnavailable { get; set; }
  }
}
