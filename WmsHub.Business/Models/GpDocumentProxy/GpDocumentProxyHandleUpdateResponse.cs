using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.GpDocumentProxy;
public class GpDocumentProxyHandleUpdateResponse
{
  public DocumentUpdateStatus DocumentUpdateStatus { get; set; }
  public string ErrorMessage { get; set; }
}
