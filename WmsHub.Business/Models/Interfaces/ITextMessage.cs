using System;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models;

public interface ITextMessage : IBaseModel
{
  int DobAttempts { get; set; }
  bool HasDoNotContactOutcome { get; }
  string Number { get; set; }
  string Outcome { get; set; }
  DateTimeOffset Received { get; set; }
  Referral Referral { get; set; }
  Guid ReferralId { get; set; }
  public string ReferralStatus { get; set; }
  DateTimeOffset Sent { get; set; }
  string ServiceUserLinkId { get; set; }
}