using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Models.ProviderService;

namespace WmsHub.Business.Services;

public interface IWmsAuthService: IServiceBase
{
  Task<AccessTokenResponse> GenerateTokensAsync();
  Task<Provider> GetProviderAsync();
  Task<ProviderAuthNewKeyResponse> SendNewKeyAsync();
  Task UpdateProviderAuthKeyAsync(Provider model);
  Task<KeyValidationResponse> ValidateKeyAsync(string key);
}