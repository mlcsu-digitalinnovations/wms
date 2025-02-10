using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Entities.ErsMock;

public class ErsMockReferral
{
  [Required]
  public bool IsActive { get; set; }
  public string AttachmentId { get; set; }
  public DateTimeOffset? Creation { get; set; }
  [Required]
  public string Description { get; set; }
  [Required]
  [MaxLength(10)]
  public string FileExtension { get; set; }
  [Required]
  public bool IsTriaged { get; set; } = false;
  [Required]
  public string NhsNumber { get; set; }
  public string ReviewOutcome { get; set; }
  [Required]
  public string ServiceId { get; set; }
  [Key]
  public string Ubrn { get; set; }
}
