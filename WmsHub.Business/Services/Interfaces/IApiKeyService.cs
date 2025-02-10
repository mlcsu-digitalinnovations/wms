using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models.Authentication;

namespace WmsHub.Business.Services.Interfaces
{
  public interface IApiKeyService : IServiceBase
  {
    Task<ApiKeyStoreResponse> Validate(string key,
      bool validateProviders = false);
    Task<ApiKeyStoreResponse> GetApiKeyStoreByKeyAsync(string key);
    Task<KeyValuePair<Guid, string>> ValidateProviderKeyAsync(string key);
  }
}
