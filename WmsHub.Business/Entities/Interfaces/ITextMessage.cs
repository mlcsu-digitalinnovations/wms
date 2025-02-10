using System;

namespace WmsHub.Business.Entities
{
  public interface ITextMessage
  {
    Guid ReferralId { get; set; }
    string Number { get; set; }
    DateTimeOffset Sent { get; set; }
    DateTimeOffset? Received { get; set; }
    string Outcome { get; set; }
    public int DobAttempts { get; set; }
  }
}