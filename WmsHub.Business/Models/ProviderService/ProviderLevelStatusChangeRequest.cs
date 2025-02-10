using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models.ProviderService
{
  public class ProviderLevelStatusChangeRequest
  {
    [Required]
    public Guid Id { get; set; }
    [Required]
    public bool? Level1 { get; set; }
    [Required]
    public bool? Level2 { get; set; }
    [Required]
    public bool? Level3 { get; set; }
  }
}
