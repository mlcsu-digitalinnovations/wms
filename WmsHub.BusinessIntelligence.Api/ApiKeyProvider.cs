using AspNetCore.Authentication.ApiKey;
using Serilog;
using System;
using System.Threading.Tasks;
using WmsHub.BusinessIntelligence.Api.AuthPolicies;
using WmsHub.Common.AuthPolicies;

namespace WmsHub.BusinessIntelligence.Api;

public class ApiKeyProvider : IApiKeyProvider
{
  public Task<IApiKey> ProvideAsync(string key)
  {
    try
    {
      foreach (IAuthPolicy authPolicy in AuthorizationPolicies.AuthPolicies)
      {
        if (authPolicy.HasMatchingApiKey(key))
        {
          ApiKey apiKey = new(
            authPolicy.ApiKeyValue,
            authPolicy.Owner,
            authPolicy.GetClaims(key));

          return Task.FromResult<IApiKey>(apiKey);
        }
      }

      Log.Information("Unknown ApiKey '{ApiKey}'.", key);
      return Task.FromResult<IApiKey>(null);
    }
    catch (Exception exception)
    {
      Log.Error(exception, exception.Message);
      throw;
    }
  }
}