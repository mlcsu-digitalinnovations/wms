using System;
using System.Collections.Generic;

namespace WmsHub.BusinessIntelligence.Api.Models
{
  public class ProviderBiData
  {
    public string Name { get; set; }
    public int NoOfReferralsAwaitingAcceptance { get; set; }
    public DateTimeOffset? OldestReferralAwaitingAcceptance { get; set; }
    public IEnumerable<ProviderBiDataRequestError> RequestErrors { get; set; }
  }
}