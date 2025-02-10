using System.ComponentModel.DataAnnotations;

namespace WmsHub.ErsMock.Api.Models;

public class ErsMockApiOptions
{
  public const string ConfigSectionPath = nameof(ErsMockApiOptions);

  [Required]
  public required string AttachmentReferralLetterPath { get; set; }
}
