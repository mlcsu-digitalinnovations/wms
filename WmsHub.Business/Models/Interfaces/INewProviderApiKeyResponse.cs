using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ProviderService
{
  public interface INewProviderApiKeyResponse
  {
    string ApiKey { get; set; }
    List<string> Errors { get; }
    StatusType ResponseStatus { get; set; }

    string GetErrorMessage();
  }
}