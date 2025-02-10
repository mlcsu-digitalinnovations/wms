using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ProviderService;

public class ProviderOptions
{ 
  public static DomainAccess Access => DomainAccess.ProviderApi;    
  [Required]
  public int CompletionDays { get; set; } = 84;
  [Required]
  public int DischargeAfterDays { get; set; } = 94;
  [Required]
  public int DischargeCompletionDays { get; set; } = 49;
  [Required]
  public bool IgnoreStatusRequirementForUpdate { get; set; }
  public int MaxCompletionDays => CompletionDays + NumDaysPastCompletedDate;
  [Required]
  public int NumDaysPastCompletedDate { get; set; } = 10;
  public static string SectionKey => "ProviderOptions";
  [Required]
  public decimal WeightChangeThreshold { get; set; } = 25;
}
