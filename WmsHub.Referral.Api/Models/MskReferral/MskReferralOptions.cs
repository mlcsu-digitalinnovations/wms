using System.Collections.Generic;

namespace WmsHub.Referral.Api.Models.MskReferral
{
  public class MskReferralOptions
  {
    public virtual Dictionary<string, string> MskHubs { get; set; } = new ();

    public virtual bool IsWhitelistEnabled { get; set; } = true;

    public virtual bool WhitelistHasValues => MskHubs?.Count > 0;

    public const string SectionKey = "MskReferralOptions";
  }
}
