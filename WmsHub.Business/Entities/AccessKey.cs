using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Entities;

public class AccessKey
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }
  [Required]
  public string Key { get; set; }
  [Required]
  public string Email { get; set; }
  [Required]
  public DateTimeOffset Expires { get; set; }
  [Required]
  public int TryCount { get; set; }
  [Required]
  public AccessKeyType Type { get; set; }
}
