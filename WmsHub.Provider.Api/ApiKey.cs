using AspNetCore.Authentication.ApiKey;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace WmsHub.Provider.Api
{
  [ExcludeFromCodeCoverage]
  class ApiKey : IApiKey
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">Apikey as String</param>
    /// <param name="owner">String</param>
    /// <param name="claims">List of System.Security.Claims.Claim </param>
    public ApiKey(string key, string owner, List<Claim> claims = null)
    {
      Key = key;
      OwnerName = owner;
      Claims = claims ?? new List<Claim>();
    }

    public string Key { get; }
    public string OwnerName { get; }
    public IReadOnlyCollection<Claim> Claims { get; }
  }
}