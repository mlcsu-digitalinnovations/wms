using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Entities;

public class ProviderDetailBase : BaseEntity
{
  [Required]
  public Guid ProviderId { get; set; }

  [Required]
  public string Section { get; set; }

  [Required]
  public int TriageLevel { get; set; }

  [Required]
  public string Value { get; set; }
}
