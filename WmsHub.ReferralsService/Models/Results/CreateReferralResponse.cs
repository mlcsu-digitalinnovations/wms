using System;

namespace WmsHub.ReferralsService.Models.Results;

public class CreateReferralResponse
{
  public Guid Id { get; set; }
  public string Status { get; set; }
}
