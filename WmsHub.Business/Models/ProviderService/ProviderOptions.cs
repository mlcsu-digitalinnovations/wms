using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models
{
  public class ProviderOptions
  {
    public const string SectionKey = "ProviderOptions";
    [Required]
    public int CompletionDays { get; set; } = 84;
    [Required]
    public int DischargeAfterDays { get; set; } = 94;
    [Required]
    public int DischargeCompletionDays { get; set; } = 49;
    [Required]
    public int NumDaysPastCompletedDate { get; set; } = 10;
    public int MaxCompletionDays => CompletionDays + NumDaysPastCompletedDate;
    public DomainAccess Access => DomainAccess.ProviderApi;
    public bool IgnoreStatusRequirementForUpdate { get; set; } = false;
    public decimal WeightChangeThreshold { get; set; } = 25;
  }
}
