using System.ComponentModel.DataAnnotations;

namespace WmsHub.ReferralsService.SmartCard.Configuration
{
  public class Config
  {
    [Required]
    [Range(1, 10)]
    public int AttemptsToLogin { get; internal set; } = 5;

    public RpaConfig RpaSettings { get; set; }
  }
}
