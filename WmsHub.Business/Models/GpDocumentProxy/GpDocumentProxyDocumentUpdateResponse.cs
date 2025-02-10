using System;

namespace WmsHub.Business.Models.GpDocumentProxy;

public class GpDocumentProxyDocumentUpdateResponse
{
  public string DocumentStatus { get; set; }
  public string Information { get; set; }
  public Guid ReferralId { get; set; }
  public string UpdateStatus { get; set; }
}