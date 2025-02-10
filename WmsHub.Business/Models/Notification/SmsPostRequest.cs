using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models;

public class SmsPostRequest
{
  [Required]
  [MaxLength(200)]
  public string ClientReference { get; set; }
  [Required]
  public string Mobile { get; set; }
  [Required]
  public Dictionary<string, dynamic> Personalisation { get; set; }
  [Required]
  public string TemplateId { get; set; }
  public string SenderId { get; set; }
}
