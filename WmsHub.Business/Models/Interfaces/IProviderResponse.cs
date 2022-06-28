using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ProviderService
{
  public interface IProviderResponse
  {
    StatusType ResponseStatus { get; set; }
    string GetErrorMessage();
  }
}