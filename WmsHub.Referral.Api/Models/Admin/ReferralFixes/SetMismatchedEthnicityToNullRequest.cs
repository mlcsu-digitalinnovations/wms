using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models.Admin.ReferralFixes;

public class SetMismatchedEthnicityToNullRequest
{
  [Required]
  [MinLength(1, ErrorMessage = "Ids array must include at least one Id.")]
  public Guid[] Ids { get; set; }
}
