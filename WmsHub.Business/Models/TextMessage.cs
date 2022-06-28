using System;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Models
{
  public class TextMessage : BaseModel, ITextMessage
  {
    public string Base36DateSent { get; set; }
    public Guid ReferralId { get; set; }
    public string Number { get; set; }
    public DateTimeOffset Sent { get; set; }
    public DateTimeOffset Received { get; set; }
    public string Outcome { get; set; }

    public Referral Referral { get; set; }
    public bool HasDoNotContactOutcome => 
      Outcome == Constants.DO_NOT_CONTACT_EMAIL;
  }
}