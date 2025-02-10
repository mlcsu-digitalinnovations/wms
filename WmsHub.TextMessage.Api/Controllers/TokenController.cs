using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.Notify;
using WmsHub.Common.Extensions;
using WmsHub.TextMessage.Api.Models;

namespace WmsHub.TextMessage.Api.Controllers
{
  [ExcludeFromCodeCoverage(Justification = 
    "Obsolete controller should be using the ProviderAuth")]
  [ApiController]
  [ApiExplorerSettings(IgnoreApi = true)]
  [ApiVersion("1.0")]
  [Route("v{version:apiVersion}/[controller]")]
  [Route("[controller]")]
  public class TokenController : ControllerBase
  {
    private readonly TextOptions _options;

    public TokenController(IOptions<TextOptions> options)
    {
      _options = options.Value;
    }

    /// <summary>
    /// Returns UserId from Claim
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public Guid Get()
    {
      var uuid = User.GetUserId();

      return uuid;
    }

    [HttpPost("validate")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> Validate(string token)
    {
      bool handler = await NotifyTokenHandler.ValidateCurrentToken(token);
      return Ok(handler);
    }

    /// <summary>
    /// Produces the token for end users.
    /// </summary>
    /// <param name="username">Name</param>
    /// <param name="uuid">User ID</param>
    /// <param name="valid">
    /// Number of days from today the token will last</param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "ApiKey")]    
    public IActionResult Post(string username, string uuid, int? valid)
    {
      if (valid > 3650)
        return Problem(
          detail: "Token cannot be created to be valid for more " + 
            "than 3650 days.",
          statusCode: StatusCodes.Status400BadRequest);

      if (!_options.TokenEnabled)
        return NotFound();

      int minutes = (int)(valid == null ? 5256000 : valid * 24 * 60);
      try
      {
        string token =
          NotifyTokenHandler.GenerateToken(username, uuid, minutes);
        JsonWebToken tokenObject = new JsonWebTokenHandler()
          .ReadToken(token) as JsonWebToken;

        var expiryDate = tokenObject.ValidTo.ToLocalTime();

        GetTokenResponse response = new GetTokenResponse
          {Token = token, Expiry = expiryDate };

        return Ok(response);
      }
      catch (Exception)
      {
       return Problem("There was a problem getting a new token");
      }

    }
  }

}
