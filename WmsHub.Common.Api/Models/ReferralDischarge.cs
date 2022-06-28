using System;

namespace WmsHub.Common.Api.Models
{
  public class ReferralDischarge
  {
    public Guid Id { get; set; }
    public string Ubrn { get; set; }
    public string NhsNumber { get; set; }
    public string DischargeMessage { get; set; }
  }
}
