using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Models.AuthService;
using WmsHub.Business.Models.Notify;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Models.Authentication
{
  public static class NotifyTokenHandler
  {
    private static ITextOptions _options;
    private static string _connectionString;
    private static IHttpContextAccessor _accessor;
    private const string _ignoreIpCheck = "**IGNORE_FOR_TESTING**";

    private static readonly string _sidClaim =
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid";

    public static void Configure(IOptions<ITextOptions> options,
      string connectionString,
      IHttpContextAccessor accessor)
    {
      _options = options.Value;
      _connectionString = connectionString;
      _accessor = accessor;
    }

    public static string GenerateToken(string nameIdentifier,
      string sid, int minutes = 1440)
    {
     
      if (!Guid.TryParse(sid, out Guid userId) || userId == Guid.Empty)
        throw new ArgumentException($"{sid} is not a valid user id");

      SymmetricSecurityKey mySecurityKey = new SymmetricSecurityKey(
        Encoding.ASCII.GetBytes(_options.TokenSecret));

      JsonWebTokenHandler tokenHandler = new();
      SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new Claim[]
        {
          new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
          new Claim(ClaimTypes.Sid, sid),
        }),
        Expires = DateTime.UtcNow.AddMinutes(minutes),
        Issuer = _options.Issuer,
        Audience = _options.Audience,
        SigningCredentials =
          new SigningCredentials(mySecurityKey,
            SecurityAlgorithms.HmacSha256Signature)
      };

      return tokenHandler.CreateToken(tokenDescriptor);
    }

    public static TokenValidationParameters GetTokenValidationParameters()
    {
      SymmetricSecurityKey mySecurityKey =
        new SymmetricSecurityKey(
          Encoding.ASCII.GetBytes(_options.TokenSecret));
      return new TokenValidationParameters
      {

        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = _options.Issuer,
        ValidAudience = _options.Audience,
        ValidateLifetime = true,
        IssuerSigningKey = mySecurityKey
        //RequireExpirationTime = true
      };

    }

    public async static Task<bool> ValidateCurrentToken(string token)
    {
      SymmetricSecurityKey mySecurityKey = new(Encoding.ASCII.GetBytes(_options.TokenSecret));
      JsonWebTokenHandler jsonWebTokenHandler = new();
      TokenValidationResult tokenValidationResult = await jsonWebTokenHandler.ValidateTokenAsync(
        token,
        new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidIssuer = _options.Issuer,
          ValidAudience = _options.Audience,
          IssuerSigningKey = mySecurityKey,
          RequireExpirationTime = true,
        });

      try
      {
        JsonWebToken jsonWebToken = tokenValidationResult.SecurityToken as JsonWebToken;

        string sid = jsonWebToken.Claims
          .First(claim => claim.Type =="http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")
          .Value;

        if (!_options.ValidUsers.Contains(sid))
        {
          throw new ArgumentException($"sid of {sid} is not valid");
        }

      }
      catch
      {
        return false;
      }

      return true;
    }


    public static string GetClaim(string token, string claimType)
    {
      JsonWebTokenHandler tokenHandler = new();
      JsonWebToken securityToken =
        tokenHandler.ReadToken(token) as JsonWebToken;

      string stringClaimValue = securityToken.Claims.First(
        claim => claim.Type == claimType).Value;
      return stringClaimValue;
    }

    public static JwtBearerEvents GetEvents()
    {
      JwtBearerEvents events = new JwtBearerEvents
      {
        OnTokenValidated = ctx =>
        {

          // Check if the user has an OID claim
          if (!ctx.Principal.HasClaim(c => c.Type == _sidClaim))
          {
            string msg = "The claim 'sid' is not present in the token.";

            return TokenProblem(ctx, msg);
          }

          if (ctx.SecurityToken.ValidTo < DateTimeOffset.Now)
          {
            string msg = "Token is out of date.";
            return TokenProblem(ctx, msg);
          }


          string sid = ctx.Principal.Claims
           .First(claim => claim.Type == _sidClaim).Value;

          //Validate SID
          if (!Guid.TryParse(sid, out Guid userId) || userId == Guid.Empty)
          {
            string msg = "The sid is not in the correct format.";
            return TokenProblem(ctx, msg);
          }


          if (_options.ValidUsers != null)
          {
            if (_options.ValidUsers.Contains(sid))
              return Task.CompletedTask;
          }

          using (DatabaseContext context =
            new DatabaseContext(_connectionString))
          {
            Entities.Provider provider = context.Providers
             .Include(t => t.ProviderAuth)
             .AsNoTracking()
             .SingleOrDefault(t => t.Id == userId && t.IsActive);

            if (provider == null)
            {
              string msg = $"Provider {userId} not found.";
              return TokenProblem(ctx, msg);
            }

            // Azure front door adds X-Azure-ClientIP for the originating IP
            // address and the front door ip address, but DOES NOT overwrite
            // X-Azure-ClientIP if in original request
            // Check Azure Front Door if this can be changed
            string xAzureClientIp = AuthServiceHelper
              .GetHeaderValueAs<string>("X-Azure-SocketIP");
            string ipToValidate;
            if (xAzureClientIp == default)
            {
              ipToValidate = _accessor
                .HttpContext.Connection.RemoteIpAddress.ToString();
            }
            else
            {
              List<string> xAzureClientIpList = xAzureClientIp
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

              // first ip address in the X-Azure-ClientIP
              // should be the client IP
              ipToValidate = xAzureClientIpList.First();
            }

            //Validate Against Whitelist
            string[] whiteList = provider.ProviderAuth.IpWhitelist
              .Split(',', StringSplitOptions.RemoveEmptyEntries);
            //Build list from providerauthList
            List<string> validIPv4List = whiteList.TryParseIpWhitelist();

            //Skip if **IGNORE_FOR_TESTING** is in whitelist

            if (!whiteList.Contains(_ignoreIpCheck)
                && (!validIPv4List.Any()
                  || !validIPv4List.Contains(ipToValidate)))
            {
              string msg = $"IP address {ipToValidate} is unknown, " +
                           $"removing access token for user {userId}.";
              provider.ProviderAuth.AccessToken = string.Empty;
              provider.ModifiedAt = DateTimeOffset.Now;
              provider.ModifiedByUserId = userId;
              context.SaveChangesAsync();
              return TokenProblem(ctx, msg);
            }

            string encodedToken = ((JsonWebToken)ctx.SecurityToken).EncodedToken;

            if (provider.ProviderAuth.AccessToken != encodedToken)
            {
              string msg = $"Token is invalid for user {userId}.";
              return TokenProblem(ctx, msg);
            }
          }

          return Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {

          ctx.NoResult();
          ctx.Response.StatusCode = (int) HttpStatusCode.Unauthorized;

          if (ctx.Exception.GetType() ==
              typeof(SecurityTokenExpiredException))
          {
            ctx.Response.Headers.Append("Token-Expired", "true");
            ctx.Fail("Token-Expired");
          }
          ctx.Fail("Unknown Failure");
          return Task.CompletedTask;
        }
      };

      return events;
    }

    private static Task TokenProblem(
      TokenValidatedContext ctx, 
      string message = "")
    {

      ctx.NoResult();
      ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
      if (string.IsNullOrEmpty(message))
      {
        message = "Unknown Failure";
      }

      Log.Warning(message);
      ctx.Fail(message);
      return Task.CompletedTask;
    }

    public static RefreshToken GenerateRefreshToken(string sid, int days = 30)
    {
      using (var rng = RandomNumberGenerator.Create())
      {
        byte[] randomBytes = new byte[64];
        rng.GetBytes(randomBytes);
        return new RefreshToken
        {
          Token = Convert.ToBase64String(randomBytes),
          Expires = DateTimeOffset.Now.AddDays(days),
          Created = DateTimeOffset.Now,
          CreatedBy = sid
        };
      }
    }
  }
}
