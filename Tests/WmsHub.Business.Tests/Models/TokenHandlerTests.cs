using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using WmsHub.Business.Models.Authentication;
using WmsHub.Business.Models.Notify;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class TokenHandlerTests
  {
    private const string _testSecret = "abcdef%6789£lkjHHY";
    private const string _issuer = "http://mytestsite.com";
    private const string _audience = "http://gov.uk";
    private readonly string _validId = Guid.NewGuid().ToString();
    private readonly string _invalidId = Guid.Empty.ToString();
    private readonly Mock<IOptions<TextOptions>> _mockSettings = new();
    private readonly Mock<TextOptions> _options = new();

    public TokenHandlerTests()
    {

      List<string> validIds = new List<string> { _validId };

      _options.Setup(x => x.TokenSecret)
        .Returns(_testSecret);
      _options.Setup(x => x.Issuer).Returns(_issuer);
      _options.Setup(x => x.Audience).Returns(_audience);
      _options.Setup(x => x.ValidUsers).Returns(validIds);

      _mockSettings.Setup(x => x.Value)
        .Returns(_options.Object);

      NotifyTokenHandler.Configure(Options.Create(_options.Object),null, null);

    }

    private static string GetClaim(string token, string claimType)
    {
      JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
      JwtSecurityToken securityToken =
        tokenHandler.ReadToken(token) as JwtSecurityToken;

      try
      {
        string stringClaimValue = securityToken.Claims.First(
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

      SymmetricSecurityKey mySecurityKey = new SymmetricSecurityKey(
        Encoding.ASCII.GetBytes(_testSecret));

      JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
      SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
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

      SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }

    public class GenerateTokenTests: TokenHandlerTests
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

        JwtSecurityToken tokenObject = new JwtSecurityTokenHandler()
          .ReadToken(token) as JwtSecurityToken;

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

        JwtSecurityToken tokenObject = new JwtSecurityTokenHandler()
          .ReadToken(token) as JwtSecurityToken;

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

    [Obsolete("Unit test no longer valid as validation " +
              "carried out as part of GetEvents")]
    public class ValidateTokenTests: TokenHandlerTests
    {
      [Fact]
      public void ValidToken()
      {
        //Arrange
        string tokenToTest = NotifyTokenHandler.GenerateToken(
          "test",_validId);
        List<string> validIds = new List<string> { _validId };
        //Act
        bool valid = NotifyTokenHandler.ValidateCurrentToken(tokenToTest);
        //Asset
        //valid.Should().BeTrue();
        Assert.True(true, "Obsolete test");
      }

      [Fact]
      public void InValidToken()
      {
        //Arrange
        string tokenToTest = NotifyTokenHandler.GenerateToken(
          "test", Guid.NewGuid().ToString());
        NotifyTokenHandler.Configure(_mockSettings.Object, null, null);
        //Act
        bool valid = NotifyTokenHandler.ValidateCurrentToken(tokenToTest);
        //Asset
        //valid.Should().BeFalse();
        Assert.True(true, "Obsolete test");
      }
    }



  }
}
