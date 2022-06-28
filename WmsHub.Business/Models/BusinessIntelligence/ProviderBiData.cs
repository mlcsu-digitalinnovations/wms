using System;
using System.Collections.Generic;

namespace WmsHub.Business.Models
{
  public class ProviderBiData
  {
    public Guid ProviderId { get; set; }
    public string Name { get; set; }
    public int NoOfReferralsAwaitingAcceptance { get; set; }
    public DateTimeOffset? OldestReferralAwaitingAcceptance { get; set; }
    public IEnumerable<ProviderBiDataRequestError> RequestErrors { get; set; }
  }
}
