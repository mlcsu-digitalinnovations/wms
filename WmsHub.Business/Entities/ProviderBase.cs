using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Entities
{
  public class ProviderBase : BaseEntity
  {
    public string ApiKey { get; internal set; }
    public DateTimeOffset? ApiKeyExpires { get; internal set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Summary { get; set; }
    [Required]
    public string Summary2 { get; set; }
    [Required]
    public string Summary3 { get; set; }
    [Required]
    public string Website { get; set; }
    [Required]
    public string Logo { get; set; }
    public bool Level1 { get; set; }
    public bool Level2 { get; set; }
    public bool Level3 { get; set; }
  }
}
