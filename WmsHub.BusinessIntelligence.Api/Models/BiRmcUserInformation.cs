using System;

namespace WmsHub.BusinessIntelligence.Api.Models
{
  public class BiRmcUserInformation
  {
    public string Username { get; set; }
    public string Action { get; set; }
    public DateTimeOffset ActionDateTime { get; set; }
    public string StatusReason { get; set; }
    public string DelayReason { get; set; }
    public string Ubrn { get; set; }
  }
}
