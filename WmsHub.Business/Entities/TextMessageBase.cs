using System;

namespace WmsHub.Business.Entities;

public abstract class TextMessageBase : BaseEntity
{
  public int DobAttempts { get; set; }
  public string Number { get; set; }
  public string Outcome { get; set; }
  public DateTimeOffset? Received { get; set; }
  public Guid ReferralId { get; set; }
  public string ReferralStatus { get; set; }
  public DateTimeOffset Sent { get; set; }
  public string ServiceUserLinkId { get; set; }
}