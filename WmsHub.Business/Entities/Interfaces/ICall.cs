using System;

namespace WmsHub.Business.Entities
{
  public interface ICall
  {
    Guid ReferralId { get; set; }
    string Number { get; set; }
    DateTimeOffset Sent { get; set; }
    DateTimeOffset? Called { get; set; }
    string Outcome { get; set; }
  }
}