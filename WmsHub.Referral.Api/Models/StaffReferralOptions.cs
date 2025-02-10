namespace WmsHub.Referral.Api.Models;

public class StaffReferralOptions : EmailDomainWhitelistBase
{
  private const string REFERRAL_TYPE_NAME = "Staff";
  public const string SectionKey = "StaffReferralOptions";

  protected override string ReferralTypeName => REFERRAL_TYPE_NAME;

  public int MaxActiveAccessKeys { get; set; } = 5;
}
