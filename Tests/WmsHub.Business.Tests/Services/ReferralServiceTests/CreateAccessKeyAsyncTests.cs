using FluentAssertions;
using FluentAssertions.Execution;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.ReferralService.AccessKeys;
using Xunit;
using Xunit.Abstractions;
using static WmsHub.Business.Models.ReferralService.ResponseBase;

namespace WmsHub.Business.Tests.Services;

public partial class ReferralServiceTests : ServiceTestsBase
{
  public class CreateAccessKeyAsync : ReferralServiceTests, IDisposable
  {
    private readonly string _validCodeRegex = "^([a-zA-Z0-9]){6}$";

    public new void Dispose()
    {
      _context.AccessKeys.RemoveRange(_context.AccessKeys);
    }

    public CreateAccessKeyAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper)
      : base(serviceFixture, testOutputHelper)
    {
      Serilog.Log.Logger = new LoggerConfiguration()
        .WriteTo.TestOutput(testOutputHelper)
        .CreateLogger();

      _context.AccessKeys.RemoveRange(_context.AccessKeys);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    [InlineData("not.an.email", 2000)]
    public async Task InvalidModel_ErrorTypeValidation(
      string email,
      int expireMinutes)
    {
      // Arrange.
      CreateAccessKey createAccessKey = new()
      {
        Email = email,
        ExpireMinutes = expireMinutes
      };

      // Act.
      ICreateAccessKeyResponse response = await _service
        .CreateAccessKeyAsync(createAccessKey);

      // Assert.
      string errorMessage = response.GetErrorMessage();
      using (new AssertionScope())
      {
        response.AccessKey.Should().BeNull();
        response.Email.Should().BeNull();
        response.Expires.Should().Be(default);
        response.ErrorType.Should().Be(ErrorTypes.Validation);
        response.HasErrors.Should().BeTrue();
        errorMessage.Should().Contain(nameof(createAccessKey.Email));
        errorMessage.Should().Contain(nameof(createAccessKey.ExpireMinutes));
      }
    }

    [Theory]
    [InlineData(AccessKeyType.MskReferral)]
    [InlineData(AccessKeyType.StaffReferral)]
    public async Task NoExistingEntity_EntityCreated(
      AccessKeyType accessKeyType)
    {
      // Arrange.
      int expireMinutes = 25;
      DateTimeOffset expectedExpires = DateTimeOffset.UtcNow
        .AddMinutes(expireMinutes);

      CreateAccessKey createAccessKey = new()
      {
        Email = "NoExistingEntity@nhs.uk",
        ExpireMinutes = expireMinutes,
        AccessKeyType = accessKeyType,
        MaxActiveAccessKeys = 5
      };

      // Act.
      ICreateAccessKeyResponse response = await _service
        .CreateAccessKeyAsync(createAccessKey);

      // Assert.
      string errorMessage = response.GetErrorMessage();
      AccessKey accessKey = _context.AccessKeys
        .Single(x => x.Email.Equals(createAccessKey.Email));

      response.AccessKey.Should().MatchRegex(_validCodeRegex);
      response.Email.Should().Be(createAccessKey.Email);
      response.Expires.Should()
        .BeCloseTo(expectedExpires, TimeSpan.FromSeconds(1));
      response.ErrorType.Should().Be(ErrorTypes.None);
      response.HasErrors.Should().BeFalse();
      errorMessage.Should().Be(string.Empty);

      accessKey.Key.Should().MatchRegex(_validCodeRegex);
      accessKey.Email.Should().Be(createAccessKey.Email);
      accessKey.Expires.Should()
        .BeCloseTo(expectedExpires, TimeSpan.FromSeconds(1));
      accessKey.TryCount.Should().Be(0);
    }

    [Theory]
    [InlineData(10, AccessKeyType.MskReferral)]
    [InlineData(10, AccessKeyType.StaffReferral)]
    public async Task ExistingEntity_EntityCreated(
      int expiresIn,
      AccessKeyType accessKeyType)
    {
      // Arrange.     
      AccessKey existingKey = new()
      {
        Key = "123456",
        Email = "ExistingEntity@nhs.uk",
        Expires = DateTimeOffset.UtcNow.AddMinutes(expiresIn),
        TryCount = 1,
        Type = accessKeyType
      };
      _context.Add(existingKey);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      int expireMinutes = 25;
      DateTimeOffset expectedExpires = DateTimeOffset.UtcNow
        .AddMinutes(expireMinutes);

      CreateAccessKey createAccessKey = new()
      {
        Email = existingKey.Email,
        ExpireMinutes = expireMinutes,
        AccessKeyType = accessKeyType,
        MaxActiveAccessKeys = 5
      };

      // Act.
      ICreateAccessKeyResponse response = await _service
        .CreateAccessKeyAsync(createAccessKey);

      // Assert.
      string errorMessage = response.GetErrorMessage();
      AccessKey accessKey = _context.AccessKeys
        .Single(x => x.Email.Equals(createAccessKey.Email)
          && x.Key != existingKey.Key);

      response.AccessKey.Should().NotBe(existingKey.Key).And.MatchRegex(_validCodeRegex);
      response.Email.Should().Be(createAccessKey.Email);
      response.Expires.Should()
        .BeCloseTo(expectedExpires, TimeSpan.FromSeconds(10));
      response.ErrorType.Should().Be(ErrorTypes.None);
      response.HasErrors.Should().BeFalse();
      errorMessage.Should().Be(string.Empty);

      accessKey.Key.Should()
        .NotBe(existingKey.Key)
        .And.Be(response.AccessKey);
      accessKey.Email.Should().Be(response.Email);
      accessKey.Expires.Should()
        .BeCloseTo(expectedExpires, TimeSpan.FromSeconds(1));
      accessKey.TryCount.Should().Be(0);
    }

    [Theory]
    [InlineData(AccessKeyType.MskReferral)]
    [InlineData(AccessKeyType.StaffReferral)]
    public async Task ExistingEntityExpired_Deleted(
      AccessKeyType accessKeyType)
    {
      // Arrange.     
      AccessKey existingKey = new()
      {
        Key = "123456",
        Email = "ExistingEntityExpired@nhs.uk",
        Expires = DateTimeOffset.UtcNow.AddDays(-1),
        TryCount = 0
      };
      _context.Add(existingKey);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      int expireMinutes = 25;
      DateTimeOffset expectedExpires = DateTimeOffset.UtcNow
        .AddMinutes(expireMinutes);

      CreateAccessKey createAccessKey = new()
      {
        Email = "NewEntity@nhs.uk",
        ExpireMinutes = expireMinutes,
        MaxActiveAccessKeys = 5,
        AccessKeyType = accessKeyType
      };

      // Act.
      ICreateAccessKeyResponse response = await _service
        .CreateAccessKeyAsync(createAccessKey);

      // Assert.
      string errorMessage = response.GetErrorMessage();
      AccessKey accessKey = _context.AccessKeys
        .Single(x => x.Email.Equals(createAccessKey.Email));

      response.AccessKey.Should().NotBe(existingKey.Key).And.MatchRegex(_validCodeRegex);
      response.Email.Should().Be(createAccessKey.Email);
      response.Expires.Should()
        .BeCloseTo(expectedExpires, TimeSpan.FromSeconds(1));
      response.ErrorType.Should().Be(ErrorTypes.None);
      response.HasErrors.Should().BeFalse();
      errorMessage.Should().Be(string.Empty);

      accessKey.Key.Should()
        .NotBe(existingKey.Key)
        .And.Be(response.AccessKey);
      accessKey.Email.Should().Be(response.Email);
      accessKey.Expires.Should()
        .BeCloseTo(expectedExpires, TimeSpan.FromSeconds(1));
      accessKey.TryCount.Should().Be(0);

      // Existing expired key should have been deleted.
      _context.AccessKeys.Any(x => x.Email.Equals(existingKey.Email))
        .Should().BeFalse();
    }

    [Theory]
    [InlineData(AccessKeyType.MskReferral)]
    [InlineData(AccessKeyType.StaffReferral)]
    public async Task MaxActiveAccessKeys_ErrorTypeValidation(
      AccessKeyType accessKeyType)
    {
      // Arrange.
      string expectedMessage =
        "You have requested too many tokens, " +
        "please wait for the emails containing the token to arrive.";

      AccessKey existingKey1 = new()
      {
        Key = "123456",
        Email = "ExistingEntity@nhs.uk",
        Expires = DateTimeOffset.UtcNow.AddMinutes(10),
        TryCount = 1,
        Type = accessKeyType
      };
      _context.Add(existingKey1);
      AccessKey existingKey2 = new()
      {
        Key = "123457",
        Email = "ExistingEntity@nhs.uk",
        Expires = DateTimeOffset.UtcNow.AddMinutes(10),
        TryCount = 1,
        Type = accessKeyType
      };
      _context.Add(existingKey2);
      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      CreateAccessKey createAccessKey = new()
      {
        Email = "ExistingEntity@nhs.uk",
        ExpireMinutes = 10,
        AccessKeyType = accessKeyType,
        MaxActiveAccessKeys = 2
      };

      // Act.
      ICreateAccessKeyResponse response = await _service
        .CreateAccessKeyAsync(createAccessKey);

      // Assert.
      string errorMessage = response.GetErrorMessage();
      using (new AssertionScope())
      {
        response.AccessKey.Should().BeNull();
        response.Email.Should().BeNull();
        response.Expires.Should().Be(default);
        response.ErrorType.Should().Be(ErrorTypes.MaxActiveAccessKeys);
        response.HasErrors.Should().BeTrue();
        errorMessage.Should().Be(expectedMessage);
      }
    }
  }
}