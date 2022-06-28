using System;
using System.Security.Claims;

namespace WmsHub.Common.Extensions
{
  public static class ClaimsPrincipalExtensions
  {
    public const string CLAIM_OID =
      "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
      if (Guid.TryParse(claimsPrincipal
        ?.FindFirst(ClaimTypes.Sid)?.Value, out Guid userSid))
      {
        return userSid;
      }
      else
      {
        if (Guid.TryParse(claimsPrincipal
          ?.FindFirst(CLAIM_OID)?.Value, out Guid userAzureOid))
        {
          return userAzureOid;
        }
        else
        {
          return Guid.Empty;
        }        
      }
    }
  }
}