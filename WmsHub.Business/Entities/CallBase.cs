using System;

namespace WmsHub.Business.Entities
{
  public abstract class CallBase : BaseEntity
  {
    public Guid ReferralId { get; set; }
    public string Number { get; set; }
    public DateTimeOffset Sent { get; set; }
    public DateTimeOffset? Called { get; set; }
    public string Outcome { get; set; }
  }
}