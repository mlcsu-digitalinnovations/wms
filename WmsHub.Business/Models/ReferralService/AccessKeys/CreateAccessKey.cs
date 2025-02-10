using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.ReferralService.AccessKeys;

public class CreateAccessKey : ICreateAccessKey
{
  [Required]
  public AccessKeyType AccessKeyType { get; set; }
  [Required, Range(1, 1440)]
  public int ExpireMinutes { get; set; }
  public int MaxActiveAccessKeys { get; set; }
  [Required]
  [EmailAddress]
  public string Email { get; set; }
}