using System;
using System.Diagnostics.CodeAnalysis;

namespace WmsHub.Common.Api.Models
{
  [ExcludeFromCodeCoverage]
  public class GetActiveUbrnResponse
  {
    public DateTimeOffset? CriLastUpdated { get; set; }
    public long? ReferralAttachmentId { get; set; }
    public long? MostRecentAttachmentId { get; set; }
    public string Status { get; set; }
    public string Ubrn { get; set; }
    public string ServiceId { get; set; }
  }
}
