using System.Collections.Generic;

namespace WmsHub.Business.Models.GpDocumentProxy;

public class GpDocumentProxyPostBadRequest
{
  public Dictionary<string, string[]> Errors { get; set; }
  public string Title { get; set; }
}