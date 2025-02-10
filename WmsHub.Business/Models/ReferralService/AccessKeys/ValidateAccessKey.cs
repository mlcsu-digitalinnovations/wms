using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.ReferralService.AccessKeys;

public class ValidateAccessKey : IValidateAccessKey
{
  public AccessKeyType Type { get; set; }
  public int MaxActiveAccessKeys { get; set; }
  [Required]
  public string AccessKey { get; set; }
  [Required]
  [EmailAddress]
  public string Email { get; set; }
}