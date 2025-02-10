using System;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Models;

public class TextMessage : BaseModel, ITextMessage
{
  public int DobAttempts { get; set; }
  public bool HasDoNotContactOutcome =>
    Outcome == Constants.DO_NOT_CONTACT_EMAIL;
  public string Number { get; set; }  
  public string Outcome { get; set; }
  public DateTimeOffset Received { get; set; }
  public Referral Referral { get; set; }
  public Guid ReferralId { get; set; }
  public string ReferralStatus { get; set; }
  public DateTimeOffset Sent { get; set; }
  public string ServiceUserLinkId { get; set; }
}