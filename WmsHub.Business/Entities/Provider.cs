using System.Collections.Generic;
using WmsHub.Business.Entities.Interfaces;

namespace WmsHub.Business.Entities
{
  public class Provider : ProviderBase, IProvider
  {
    public virtual List<ProviderDetail> Details { get; set; }

    public virtual ProviderAuth ProviderAuth { get; set; }

    public virtual List<ProviderSubmission> ProviderSubmissions { get; set; } = new();

    public virtual List<Referral> Referrals { get; set; }

    public virtual List<RefreshToken> RefreshTokens { get; set; }
  }
}
