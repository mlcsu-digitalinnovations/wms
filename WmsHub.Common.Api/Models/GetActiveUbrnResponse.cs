using System;
using WmsHub.Common.Enums;

namespace WmsHub.Common.Api.Models;

public class GetActiveUbrnResponse
{
  private string _status;

  public DateTimeOffset? CriLastUpdated { get; set; }
  public ErsReferralStatus ErsReferralStatus { get; private set; }
  public bool IsAwaitingUpdate =>
    ErsReferralStatus == ErsReferralStatus.AwaitingUpdate;
  public bool IsInProgress => ErsReferralStatus == ErsReferralStatus.InProgress;
  public bool IsOnHold => ErsReferralStatus == ErsReferralStatus.OnHold;
  public DateTimeOffset? MostRecentAttachmentDate { get; set; }
  public string ReferralAttachmentId { get; set; }
  public string ServiceId { get; set; }
  public string Status
  {
    get => _status;
    set
    {
      _status = value;
      ErsReferralStatus = Enum.TryParse(
        Status,
        ignoreCase: true,
        out ErsReferralStatus ersReferralStatus)
          ? ersReferralStatus
          : ErsReferralStatus.Undefined;
    }
  }
  public string Ubrn { get; set; }

}
