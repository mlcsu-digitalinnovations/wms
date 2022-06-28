using System;

namespace WmsHub.Business.Models
{
  public class ProviderBiDataRequestError
  {
    public DateTimeOffset Date { get; set; }
    public int NoOfBadRequests { get; set; }
  }
}
