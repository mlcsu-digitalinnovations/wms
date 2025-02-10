using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WmsHub.Business.Models.ProviderService
{
  public class ProviderRequest : IProviderRequest
  {
    [Required]
    public Guid Id { get; set; }
    [MaxLength(100)]
    public virtual string Name { get; set; }
    public virtual string Summary { get; set; }
    public virtual string Summary2 { get; set; }
    public virtual string Summary3 { get; set; }
    public virtual string Website { get; set; }
    public virtual string Logo { get; set; }
    public virtual bool? Level1 { get; set; }
    public virtual bool? Level2 { get; set; }
    public virtual bool? Level3 { get; set; }

  }
}
