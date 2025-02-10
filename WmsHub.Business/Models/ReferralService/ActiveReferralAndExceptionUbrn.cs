using System;
using System.Text.Json.Serialization;
using WmsHub.Common.Enums;
using RefStatus = WmsHub.Business.Enums.ReferralStatus;

namespace WmsHub.Business.Models.ReferralService;

public class ActiveReferralAndExceptionUbrn
{
  private string _referralStatus;

  public DateTimeOffset? CriLastUpdated { get; set; }
  public DateTimeOffset? MostRecentAttachmentDate { get; set; }
  public string ReferralAttachmentId { get; set; }
  [JsonIgnore]
  public string ReferralStatus
  {
    get => _referralStatus;
    set
    {
      _referralStatus = value;

      if (_referralStatus == RefStatus.Exception.ToString()
        || _referralStatus == RefStatus.CancelledByEreferrals.ToString())
      {
        Status = ErsReferralStatus.OnHold.ToString();
      }
      else if (_referralStatus == RefStatus.RejectedToEreferrals.ToString())
      {
        Status = ErsReferralStatus.AwaitingUpdate.ToString();
      }
      else
      {
        Status = ErsReferralStatus.InProgress.ToString();
      }
    }
  }
  public string ServiceId { get; set; }
  public string Status { get; private set; }
  public string Ubrn { get; set; }
}
