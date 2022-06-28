using System;
using System.Collections.Generic;

#nullable enable

namespace WmsHub.Business.Entities
{
  public class Provider : ProviderBase, IProvider
  {
    public virtual List<ProviderSubmission> ProviderSubmissions { get; set; }
    = new List<ProviderSubmission>();

    public virtual List<Referral> Referrals { get; set; } = null!;

    public virtual List<RefreshToken> RefreshTokens { get; set; } = null!;

    public virtual ProviderAuth? ProviderAuth { get; set; }
  }
}
