using System.Collections.Generic;

namespace WmsHub.Business.Entities.Interfaces;

public interface IProvider
{
  bool Level1 { get; set; }
  bool Level2 { get; set; }
  bool Level3 { get; set; }
  string Logo { get; set; }
  string Name { get; set; }
  string Summary { get; set; }
  string Website { get; set; }

  List<ProviderDetail> Details { get; set; }

  ProviderAuth ProviderAuth { get; set; }

  List<ProviderSubmission> ProviderSubmissions { get; set; }

  List<Referral> Referrals { get; set; }

  List<RefreshToken> RefreshTokens { get; set; }
}