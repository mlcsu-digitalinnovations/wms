using System.Collections.Generic;

namespace WmsHub.Referral.Api.Models.MskReferral;

public class MskReferralOptions : EmailDomainWhitelistBase
{
  private const string REFERRAL_TYPE_NAME = "Msk";

  public const string SectionKey = "MskReferralOptions";

  protected override string ReferralTypeName => REFERRAL_TYPE_NAME;

  public virtual bool IsMskHubWhitelistEnabled { get; set; } = true;

  public int MaxActiveAccessKeys { get; set; } = 5;
}
