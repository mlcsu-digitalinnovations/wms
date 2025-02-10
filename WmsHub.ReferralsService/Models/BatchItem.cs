using System.Diagnostics.CodeAnalysis;

namespace WmsHub.ReferralsService.Models;

[ExcludeFromCodeCoverage]
public class BatchItem
{
  public bool AlreadyExists { get; set; }
  public string ServiceId { get; set; }
  public string Ubrn { get; set; }
  public string AttachmentId { get; set; }
  public string AttachmentFileName { get; set; }
}
