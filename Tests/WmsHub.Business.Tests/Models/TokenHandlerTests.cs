using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.Notify;
using Xunit;

namespace WmsHub.Business.Tests.Models;

public class TokenHandlerTests
{
  private const string _testSecret = "abcdef%6789£lkjHHYABCDEFGHIJKLMN";
  private const string _issuer = "http://mytestsite.com";
  private const string _audience = "http://gov.uk";
  private readonly string _validId = Guid.NewGuid().ToString();
  private readonly string _invalidId = Guid.Empty.ToString();

  private readonly TextOptions _textOptions = new();

  public TokenHandlerTests()
  {
    _textOptions.TokenSecret = _testSecret;
    _textOptions.Issuer = _issuer;
    _textOptions.Audience = _audience;
    _textOptions.ValidUsers = new() { _validId };

    NotifyTokenHandler.Configure(Options.Create(_textOptions), null, null);
  }

  private static string GetClaim(string token, string claimType)
  {
    JsonWebTokenHandler tokenHandler = new();
    JsonWebToken jsonWebToken =
      tokenHandler.ReadToken(token) as JsonWebToken;

    try
    {
      string stringClaimValue = jsonWebToken.Claims.First(
        claim => claim.Type == claimType).Value;
      return stringClaimValue;
    }
    catch (Exception ex)
    {
      return ex.Message;
    }
  }

  private static string GenerateTestToken(string sid)
  {

    SymmetricSecurityKey mySecurityKey = new(
      Encoding.ASCII.GetBytes(_testSecret));

    JsonWebTokenHandler tokenHandler = new();
    SecurityTokenDescriptor tokenDescriptor = new()
    {
      Subject = new ClaimsIdentity(new Claim[]
      {
        new Claim(ClaimTypes.NameIdentifier, "test"),
        new Claim(ClaimTypes.Sid, sid),
      }),
      Expires = DateTime.UtcNow.AddMinutes(1),
      Issuer = _issuer,
      Audience = _audience,
      SigningCredentials = new SigningCredentials(mySecurityKey,
        SecurityAlgorithms.HmacSha256Signature)
    };

    return tokenHandler.CreateToken(tokenDescriptor);
  }

  public class GenerateTokenTests : TokenHandlerTests
  {
    [Fact]
    public void GenerateValidToken()
    {
      //arrange
      string search =
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid";
      string username = "test";
      int minutes = 60;
      //act
      string token =
        NotifyTokenHandler.GenerateToken(username, _validId, minutes);

      JsonWebToken tokenObject = new JsonWebTokenHandler()
        .ReadToken(token) as JsonWebToken;

      DateTimeOffset tokenExpiry = tokenObject.ValidTo;
      DateTimeOffset tokenStart = tokenObject.ValidFrom;

      var result = tokenStart.AddMinutes(minutes).Ticks - tokenExpiry.Ticks;
      if (result != 0)
      {
        result =
          tokenStart.AddMinutes(minutes + 60).Ticks - tokenExpiry.Ticks;
      }
      string sid = GetClaim(token, search);
      //assert
      sid.Should().Be(_validId);
      result.Should().Be(0);
    }

    [Fact]
    public void GenerateValidTokenIsInFuture()
    {
      //arrange
      string search =
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid";
      string username = "test";
      int minutes = 30;
      DateTime now = (DateTimeOffset.Now).DateTime;
      //act
      string token =
        NotifyTokenHandler.GenerateToken(username, _validId, minutes);

      JsonWebToken tokenObject = new JsonWebTokenHandler()
        .ReadToken(token) as JsonWebToken;

      string sid = GetClaim(token, search);
      //assert
      sid.Should().Be(_validId);
      tokenObject.ValidTo.ToLocalTime().Should().BeAfter(now);
    }

    [Fact]
    public void GenerateInValidToken()
    {
      //arrange
      string expected = $"{_invalidId} is not a valid user id";
      string search =
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid";
      string username = "test";
      int minutes = 5;
      //act
      try
      {
        string token =
          NotifyTokenHandler.GenerateToken(username, _invalidId, minutes);

        string sid = GetClaim(token, search);
        //assert
        Assert.Equal(_validId, sid);
      }
      catch (Exception ex)
      {
        Assert.Equal(expected, ex.Message);
      }
    }

    [Fact]
    public void GenerateInValidEmptyIdToken()
    {
      //arrange
      string expected = $"{null} is not a valid user id";
      const string search =
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid";
      const string username = "test";
      const int minutes = 5;
      //act
      try
      {
        string token =
          NotifyTokenHandler.GenerateToken(username, null, minutes);

        string sid = GetClaim(token, search);
        //assert
        Assert.Equal(_validId, sid);
      }
      catch (Exception ex)
      {
        Assert.Equal(expected, ex.Message);
      }
    }
  }
}
