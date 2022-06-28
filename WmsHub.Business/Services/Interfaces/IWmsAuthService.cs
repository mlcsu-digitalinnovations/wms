using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Business.Models.AuthService;

namespace WmsHub.Business.Services
{
  public interface IWmsAuthService: IServiceBase
  {
    Task<AccessTokenResponse> GenerateTokensAsync();
    Task<Provider> GetProviderAsync();
    Task<bool> SendNewKeyAsync();
    Task<Provider> UpdateProviderAuthKeyAsync(Provider model);
    Task<KeyValidationResponse> ValidateKeyAsync(string key);
  }
}