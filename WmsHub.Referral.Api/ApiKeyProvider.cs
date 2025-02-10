using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Common.AuthPolicies;
using WmsHub.Referral.Api.AuthPolicies;

namespace WmsHub.Referral.Api
{
  public class ApiKeyProvider : IApiKeyProvider
  {
    public const string OWNER_PHARMACY = "Owner.Pharmacy";
    public const string OWNER_PRACTICE = "Owner.Practice";
    public const string REFERRAL_ADMIN = "Referral.Superuser";

    private readonly IConfiguration _configuration;
    private const string PRACTICE_API_KEY = "PracticeApiKey";
    private const string PHARMACY_API_KEY = "PharmacyApiKey";
    private const string SUPERUSER_API_KEY = "ReferralSuperuserApiKey";

    public ApiKeyProvider(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public Task<IApiKey> ProvideAsync(string key)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(key))
        {
          throw new InvalidApiKeyException("ApiKey must have a value");
        }



        string configApiKey = _configuration["ApiKey"];

        if (key == _configuration["ApiKey"])
        {
          List<Claim> claims = new List<Claim>(){
            new Claim(ClaimTypes.Sid, configApiKey)
          };

          return Task.FromResult<IApiKey>(
            new ApiKey(key, "Referral.Service", claims));
        }
        else if (key == _configuration["SelfReferralApiKey"])
        {
          List<Claim> claims = new List<Claim>(){
            new Claim(ClaimTypes.Sid, configApiKey)
          };

          return Task.FromResult<IApiKey>(
            new ApiKey(key, "SelfReferral.Service", claims));
        }
        else if (key == _configuration["PharmacyReferralApiKey"])
        {
          List<Claim> claims = new List<Claim>(){
            new Claim(ClaimTypes.Sid, configApiKey)
          };

          return Task.FromResult<IApiKey>(
            new ApiKey(key, "PharmacyReferral.Service", claims));
        }
        else if (key == _configuration[PRACTICE_API_KEY])
        {
          List<Claim> claims = new List<Claim>(){
            new Claim(ClaimTypes.Sid, configApiKey)
          };

          return Task.FromResult<IApiKey>(
            new ApiKey(key, OWNER_PRACTICE, claims));
        }
        else if (key == _configuration[PHARMACY_API_KEY])
        {
          List<Claim> claims = new List<Claim>(){
            new Claim(ClaimTypes.Sid, configApiKey)
          };

          return Task.FromResult<IApiKey>(
            new ApiKey(key, OWNER_PHARMACY, claims));
        }
        else if (key == _configuration[SUPERUSER_API_KEY])
        {
          List<Claim> claims = new List<Claim>(){
            new Claim(ClaimTypes.Sid, configApiKey)
          };

          return Task.FromResult<IApiKey>(
            new ApiKey(key, REFERRAL_ADMIN, claims));
        }
        else
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