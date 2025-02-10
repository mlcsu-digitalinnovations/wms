using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Services;

namespace WmsHub.TextMessage.Api
{
  public class ApiKeyProvider : IApiKeyProvider
  {
    private readonly ITextService _service;
    private readonly IConfiguration _configuration;

    [ExcludeFromCodeCoverage]
    public ApiKeyProvider(ITextService service,
      IConfiguration configuration)
    {
      _service = service;
      _configuration = configuration;
    }

    /// <summary>
    /// As there will be several providers this allows the provided key to 
    /// be authenticated against the proveder key in the database.  
    /// This should also set the uuid of the claim from teh Provider Id
    /// </summary>
    /// <param name="key">ApiKey</param>
    /// <returns></returns>
    public async Task<IApiKey> ProvideAsync(string key)
    {
      try
      {
        string ipAddress =
          AuthServiceHelper.GetHeaderValueAs<string>("X-Azure-SocketIP");
        Log.Debug($"IP address in the X-Azure-ClientIP: {ipAddress}");
        
        string configApiKey = _configuration["ApiKey"];

        if (string.IsNullOrWhiteSpace(configApiKey))
          throw new Exception("ApiKey is missing from appSettings");

        if (key == _configuration["ApiKey"])
        {
          List<Claim> claims = new List<Claim>(){
            new Claim(ClaimTypes.Sid, configApiKey)
          };

          return await Task.FromResult<IApiKey>(
            new ApiKey(key, "TextMessage", claims));
        }
        else
        {
          Log.Information("Invalid ApiKey {ApiKey}", key);
          return await Task.FromResult<IApiKey>(null);
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