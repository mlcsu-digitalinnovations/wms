using System;

namespace WmsHub.Referral.Api.Models.Admin.ReferralFixes
{
  public class ChangeDateOfBirthRequest
  {
    public DateTimeOffset OriginalDateOfBirth { get; set; }
    public DateTimeOffset UpdatedDateOfBirth { get; set; }
  }
}