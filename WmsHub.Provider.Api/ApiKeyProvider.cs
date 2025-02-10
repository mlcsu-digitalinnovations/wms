using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Services.Interfaces;

namespace WmsHub.Provider.Api
{
  public class ApiKeyProvider : IApiKeyProvider
  {
    private readonly IApiKeyService _apiKeyService;
    private readonly IProviderService _service;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _accessor;

    public ApiKeyProvider(IProviderService service,
      IApiKeyService apiKeyService,
      IConfiguration configuration,
      IHttpContextAccessor accessor)
    {
      _service = service;
      _configuration = configuration;
      _accessor = accessor;
      _apiKeyService = apiKeyService;
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
        string xAzureSocketIp =
          AuthServiceHelper.GetHeaderValueAs<string>("X-Azure-SocketIP");
        Log.Debug($"IP address in the X-Azure-SocketIP: {xAzureSocketIp}");


        if (string.IsNullOrWhiteSpace(key))
          throw new Exception("ApiKey must have a value");

        ApiKeyStoreResponse apiKeyStore = 
          await _apiKeyService.Validate(key, true);

        if (((DomainAccess)apiKeyStore.Domain).HasFlag(DomainAccess.TestOnly))
        {
          var error = $"ApiKey was for test access only.  " +
                      $"Validation status was " +
                      $"{apiKeyStore.ValidationStatus}, and message " +
                      $"was {apiKeyStore.GetErrorMessage()}.  " +
                      $"Expected {apiKeyStore.Domains}";
          _accessor.HttpContext.Response.Headers.Append("TestOnly", error);
          _accessor.HttpContext.Response.StatusCode =
            StatusCodes.Status403Forbidden;
          return await Task.FromResult<IApiKey>(null);
        }

        if (apiKeyStore.ValidationStatus == ValidationType.Invalid)
        {
          if (apiKeyStore.HasExpired)
          {
            _accessor.HttpContext.Response.StatusCode = 
              StatusCodes.Status410Gone;
          }
          Log.Information(apiKeyStore.GetErrorMessage(), key);
          return await Task.FromResult<IApiKey>(null);
        }

        var claims1 = new List<Claim>(){
          new Claim(ClaimTypes.Sid, apiKeyStore.Sid),
          new Claim(ClaimTypes.NameIdentifier, apiKeyStore.NameIdentifier ),
        };

        return await Task.FromResult<IApiKey>(
          new ApiKey(key, apiKeyStore.KeyUser, claims1));
      }
      catch (Exception exception)
      {
        Log.Error(exception, exception.Message);
        throw;
      }
    }
  }
}