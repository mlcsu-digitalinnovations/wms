using System;
using System.Diagnostics.CodeAnalysis;
using WmsHub.ReferralsService.Interfaces;
using WmsHub.ReferralsService.Models.BaseClasses;
using WmsHub.ReferralsService.Pdf;

namespace WmsHub.ReferralsService.Models.Results;

[ExcludeFromCodeCoverage]
public class ErsReferralResult : ReferralsResult
{
  public string AttachmentId { get; set; }
  public byte[] CriDocument { get; set; }
  public DateTimeOffset? CriDocumentDate { get; set; }
  public bool CriDocumentUpdated { get; set; }
  public IErsReferral ErsReferral { get; set; }
  public bool ExportErrors { get; set; }
  public bool InteropErrors { get; set; }
  public DateTimeOffset? MostRecentAttachmentDate { get; set; }
  public bool NoValidAttachmentFound { get; set; }
  public ReferralAttachmentPdfProcessor Pdf { get; set; }
  public string ServiceIdentifier { get; set; }
  public string Ubrn { get; set; }
}
