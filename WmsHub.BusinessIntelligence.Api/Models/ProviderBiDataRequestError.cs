using System;

namespace WmsHub.BusinessIntelligence.Api.Models
{
  public class ProviderBiDataRequestError
  {
    public DateTimeOffset Date { get; set; }
    public int NoOfBadRequests { get; set; }
  }
}
