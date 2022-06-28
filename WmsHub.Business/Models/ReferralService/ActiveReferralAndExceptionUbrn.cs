using System;

namespace WmsHub.Business.Models.ReferralService
{
  public class ActiveReferralAndExceptionUbrn
  {
    public DateTimeOffset? CriLastUpdated { get; set; }
    public long? MostRecentAttachmentId { get; set; }
    public long? ReferralAttachmentId { get; set; }
    public string Status { get; set; }
    public string Ubrn { get; set; }
    public string ServiceId { get; set; }
  }
}
