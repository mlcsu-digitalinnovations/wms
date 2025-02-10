using System;

namespace WmsHub.Business.Models.GpDocumentProxy;

public class GpDocumentProxyPostResponse
{
  public string DocumentStatus { get; set; }
  public string Message { get; set; }
  public Guid ReferralId { get; set; }
}