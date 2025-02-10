using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WmsHub.Business;
using WmsHub.Common.Extensions;

public class RmcUserClaimsTransformation : IClaimsTransformation
{
  private readonly DatabaseContext _context;
  internal const string DOMAIN_RMC_UI = "Rmc.Ui";
  internal const string REQUIRED_CLAIM = "UserDomain";

  public RmcUserClaimsTransformation(DatabaseContext context)
  {
    _context = context;
  }

  public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
  {
    if (!principal.HasClaim(claim => claim.Type == REQUIRED_CLAIM))
    {
      ClaimsIdentity claimsIdentity = new ClaimsIdentity();

      Guid userId = principal.GetUserId();

      string domain = await _context.UsersStore
        .Where(u => u.IsActive)
        .Where(u => u.Id == userId)
        .Select(u => u.Domain)
        .FirstOrDefaultAsync();

      if (domain != null)
      {
        {
          claimsIdentity.AddClaim(new Claim(REQUIRED_CLAIM, domain));
        }

        principal.AddIdentity(claimsIdentity);
      }
    }

    return principal;
  }

  public static bool UserIsAuthorized(ClaimsPrincipal user)
  {
    return user.HasClaim(REQUIRED_CLAIM, DOMAIN_RMC_UI);
  }
}