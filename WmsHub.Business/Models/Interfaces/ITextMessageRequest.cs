using System;

namespace WmsHub.Business.Models.Notify
{
  public interface ITextMessageRequest
  {
    string MobileNumber { get; set; }
    Guid ReferralId { get; set; }
  }
}