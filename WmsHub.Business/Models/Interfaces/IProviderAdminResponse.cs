using System.Collections.Generic;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ProviderService
{
  public interface IProviderAdminResponse
  {
    List<string> Errors { get; }
    IEnumerable<ProviderRequest> Providers { get; set; }
    StatusType ResponseStatus { get; set; }

    string GetErrorMessage();
  }
}