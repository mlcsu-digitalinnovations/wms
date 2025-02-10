using System;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models
{
  public interface ICall : IBaseModel
  {
    Guid ReferralId { get; set; }
    string Number { get; set; }
    DateTimeOffset Sent { get; set; }
    DateTimeOffset Called { get; set; }
    string Outcome { get; set; }

    Referral Referral { get; set; }
  }
}