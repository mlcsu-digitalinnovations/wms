using System;
using System.Diagnostics.CodeAnalysis;
using WmsHub.ReferralsService.Models.BaseClasses;
using WmsHub.ReferralsService.Pdf;

namespace WmsHub.ReferralsService.Models.Results
{
  [ExcludeFromCodeCoverage]
  public class ErsReferralResult : ReferralsResult
  {
    public string Ubrn { get; set; }
    public long? AttachmentId { get; set; }
    public long? MostRecentAttachmentId { get; set; }
    public string ServiceIdentifier { get; set; }
    public ReferralAttachmentPdfProcessor Pdf { get; set; }
    public bool NoValidAttachmentFound { get; set; }
    public bool ExportErrors { get; set; }
    public bool InteropErrors { get; set; }
    public bool CriDocumentUpdated { get; set; }
    public DateTimeOffset? CriDocumentDate { get; set; }
    public byte[] CriDocument { get; set; }
  }
}
