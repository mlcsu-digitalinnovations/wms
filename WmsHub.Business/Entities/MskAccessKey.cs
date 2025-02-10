using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Entities;

public class MskAccessKey
{
  [Required]
  public string AccessKey { get; set; }

  [Key]
  public string Email { get; set; }

  [Required]
  public DateTimeOffset Expires { get; set; }

  [Required]
  public int TryCount { get; set; }
}