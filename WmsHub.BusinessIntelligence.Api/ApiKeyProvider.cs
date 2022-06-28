using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WmsHub.BusinessIntelligence.Api
{
  public class ApiKeyProvider : IApiKeyProvider
  {
    private readonly IConfiguration _configuration;

    public ApiKeyProvider(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public Task<IApiKey> ProvideAsync(string key)
    {
      try
      {
        string configApiKey = _configuration["ApiKey"];

        if (string.IsNullOrWhiteSpace(configApiKey))
          throw new Exception("ApiKey is missing from appSettings");

        if (key == _configuration["ApiKey"])
        {
          var claims = new List<Claim>(){
            new Claim(ClaimTypes.Sid, configApiKey)
          };

          return Task.FromResult<IApiKey>
            (new ApiKey(key, "BusinessIntelligence" +
            ".Service", claims));
        }
        else
        {
          Log.Information("Invalid ApiKey {ApiKey}", key);
          return Task.FromResult<IApiKey>(null);
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception, exception.Message);
        throw;
      }
    }
  }
}