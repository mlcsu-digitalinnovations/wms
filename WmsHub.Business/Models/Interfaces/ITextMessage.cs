using System;

namespace WmsHub.Business.Models
{
  public interface ITextMessage : IBaseModel
  {
    string Base36DateSent { get; set; }
    Guid ReferralId { get; set; }
    string Number { get; set; }
    DateTimeOffset Sent { get; set; }
    DateTimeOffset Received { get; set; }
    string Outcome { get; set; }
    Referral Referral { get; set; }
    bool HasDoNotContactOutcome { get; }
  }
}