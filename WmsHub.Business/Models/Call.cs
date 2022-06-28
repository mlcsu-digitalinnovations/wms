using System;

namespace WmsHub.Business.Models
{
  public class Call : BaseModel, ICall
  {
    public Guid ReferralId { get; set; }
    public string Number { get; set; }
    public DateTimeOffset Sent { get; set; }
    public DateTimeOffset Called { get; set; }
    public string Outcome { get; set; }

    public Referral Referral { get; set; }
  }
}