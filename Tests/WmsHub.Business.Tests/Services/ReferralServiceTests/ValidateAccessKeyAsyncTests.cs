using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
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
  public class ValidateAccessKeyAsync : ReferralServiceTests, IDisposable
  {
    public new void Dispose()
    {
      CleanUp();
    }

    private void CleanUp()
    {
      _context.AccessKeys.RemoveRange(_context.AccessKeys);
      _context.SaveChanges();
      _context.AccessKeys.Count().Should().Be(0);
    }

    public ValidateAccessKeyAsync(
      ServiceFixture serviceFixture,
      ITestOutputHelper testOutputHelper) 
      : base(serviceFixture, testOutputHelper)
    {
      Serilog.Log.Logger = new LoggerConfiguration()
        .WriteTo.TestOutput(testOutputHelper)
        .CreateLogger();

      CleanUp();
    }

    [Fact]
    public async Task AccessKey_Null_ValidationError()
    {
      // Arrange.
      string expectedErrorMsg = "The AccessKey field is required.";
      ValidateAccessKey validateAccessKey = new()
      {
        Email = "Entity@nhs.net"
      };

      // Act.
      IValidateAccessKeyResponse response =
        await _service.ValidateAccessKeyAsync(validateAccessKey);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<ValidateAccessKeyResponse>();
        response.ErrorType.Should().Be(ErrorTypes.Validation);
        response.HasErrors.Should().BeTrue();
        response.GetErrorMessage().Should()
          .Be(expectedErrorMsg);
      }
    }

    [Fact]
    public async Task Email_Null_ValidationError()
    {
      // Arrange.
      string expectedErrorMsg = "The Email field is required.";
      ValidateAccessKey validateAccessKey = new()
      {
        AccessKey = "123456"
      };

      // Act.
      IValidateAccessKeyResponse response =
        await _service.ValidateAccessKeyAsync(validateAccessKey);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<ValidateAccessKeyResponse>();
        response.ErrorType.Should().Be(ErrorTypes.Validation);
        response.HasErrors.Should().BeTrue();
        response.GetErrorMessage().Should()
          .Be(expectedErrorMsg);
      }
    }

    [Theory]
    [InlineData(AccessKeyType.MskReferral)]
    [InlineData(AccessKeyType.StaffReferral)]
    public async Task AccessKey_NotFound_ValidationError(
      AccessKeyType accessKeyType)
    {
      // Arrange.
      string expectedErrorMsg =
        "Access key for email NotFoundEntity@nhs.co.uk was not found.";

      ValidateAccessKey validateAccessKey = new()
      {
        AccessKey = "012345",
        Email = "NotFoundEntity@nhs.co.uk",
        Type = accessKeyType,
        MaxActiveAccessKeys = 2
      };

      // Act.
      IValidateAccessKeyResponse response =
        await _service.ValidateAccessKeyAsync(validateAccessKey);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<ValidateAccessKeyResponse>();
        response.ErrorType.Should().Be(ErrorTypes.NotFound);
        response.HasErrors.Should().BeTrue();
        response.GetErrorMessage().Should()
          .Be(expectedErrorMsg);
      }
    }

    [Theory]
    [InlineData(AccessKeyType.MskReferral)]
    [InlineData(AccessKeyType.StaffReferral)]
    public async Task AccessKey_Incorrect_ValidationError(
      AccessKeyType accessKeyType)
    {
      // Arrange.
      string expectedErrorMsg =
        $"Access key for email Entity@nhs.co.uk does not match the " +
        $"expected access key.";

      _context.Add(new AccessKey
      {
        Email = "Entity@nhs.co.uk",
        Key = "123456",
        Type = accessKeyType,
        Expires = DateTimeOffset.Now.AddMinutes(10),
        TryCount = 0
      });
      await _context.SaveChangesAsync();

      ValidateAccessKey validateAccessKey = new()
      {
        AccessKey = "012345",
        Email = "Entity@nhs.co.uk",
        Type = accessKeyType,
        MaxActiveAccessKeys = 2
      };

      // Act.
      IValidateAccessKeyResponse response =
        await _service.ValidateAccessKeyAsync(validateAccessKey);

      // Assert.
      using (new AssertionScope())
      {
        AccessKey accessKey = await _context.AccessKeys.SingleAsync(x =>
        x.Email == "Entity@nhs.co.uk"
        && x.Type == accessKeyType);
        accessKey.TryCount.Should().Be(1);

        response.Should().BeOfType<ValidateAccessKeyResponse>();
        response.ErrorType.Should().Be(ErrorTypes.Incorrect);
        response.HasErrors.Should().BeTrue();
        response.GetErrorMessage().Should()
          .Be(expectedErrorMsg);
      }

      CleanUp();
    }

    [Theory]
    [InlineData(AccessKeyType.MskReferral)]
    [InlineData(AccessKeyType.StaffReferral)]
    public async Task AccessKey_TooManyAttempts_ValidationError(
      AccessKeyType accessKeyType)
    {
      // Arrange.
      string expectedKey = "123457";
      string expectedErrorMsg =
        $"Access key for email Entity@nhs.co.uk does not match the " +
        $"expected access key, and there have been too many attempts.";

      _context.Add(new AccessKey
      {
        Email = "Entity@nhs.co.uk",
        Key = "123456",
        Type = accessKeyType,
        Expires = DateTimeOffset.Now.AddMinutes(10),
        TryCount = 3
      });
      _context.Add(new AccessKey
      {
        Email = "Entity@nhs.co.uk",
        Key = "123457",
        Type = accessKeyType,
        Expires = DateTimeOffset.Now.AddMinutes(10),
        TryCount = 3
      });
      await _context.SaveChangesAsync();

      ValidateAccessKey validateAccessKey = new()
      {
        AccessKey = "123450",
        Email = "Entity@nhs.co.uk",
        Type = accessKeyType,
        MaxActiveAccessKeys = 2
      };

      // Act.
      IValidateAccessKeyResponse response =
        await _service.ValidateAccessKeyAsync(validateAccessKey);

      // Assert.
      using(new AssertionScope())
      {
        response.Should().BeOfType<ValidateAccessKeyResponse>();
        response.ErrorType.Should().Be(ErrorTypes.TooManyAttempts);
        response.HasErrors.Should().BeTrue();
        response.GetErrorMessage().Should()
          .Be(expectedErrorMsg);

        _context.AccessKeys.Should().HaveCount(1);
        _context.AccessKeys.First().Key.Should().Be(expectedKey);
        _context.AccessKeys.First().TryCount.Should().Be(0);
      }

      CleanUp();
    }

    [Theory]
    [InlineData(AccessKeyType.MskReferral)]
    [InlineData(AccessKeyType.StaffReferral)]
    public async Task AccessKey_Expired_ValidationError(
      AccessKeyType accessKeyType)
    {
      // Arrange.
      DateTimeOffset expires = DateTimeOffset.Now.AddMinutes(-1);
      string expected =
          $"Access key for email Entity@nhs.co.uk expired on " +
          $"{expires}.";

      _context.Add(new AccessKey
      {
        Email = "Entity@nhs.co.uk",
        Key = "123456",
        Type = accessKeyType,
        Expires = expires,
        TryCount = 3
      });
      await _context.SaveChangesAsync();

      ValidateAccessKey validateAccessKey = new()
      {
        AccessKey = "123456",
        Email = "Entity@nhs.co.uk",
        Type = accessKeyType
      };

      // Act.
      IValidateAccessKeyResponse response =
        await _service.ValidateAccessKeyAsync(validateAccessKey);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<ValidateAccessKeyResponse>();
        response.ErrorType.Should().Be(ErrorTypes.Expired);
        response.HasErrors.Should().BeTrue();
        response.GetErrorMessage().Should().Be(expected);
      }

      CleanUp();
    }

    [Theory]
    [InlineData(AccessKeyType.MskReferral, "123450")]
    [InlineData(AccessKeyType.MskReferral, "123451")]
    [InlineData(AccessKeyType.MskReferral, "123452")]
    [InlineData(AccessKeyType.StaffReferral, "123450")]
    [InlineData(AccessKeyType.StaffReferral, "123451")]
    [InlineData(AccessKeyType.StaffReferral, "123452")]
    public async Task AccessKey_Valid(
      AccessKeyType accessKeyType,
      string key)
    {
      // Arrange.
      _context.Add(new AccessKey
      {
        Email = "Entity@nhs.co.uk",
        Key = "123450",
        Type = accessKeyType,
        Expires = DateTimeOffset.Now.AddMinutes(10),
        TryCount = 0
      });
      _context.Add(new AccessKey
      {
        Email = "Entity@nhs.co.uk",
        Key = "123451",
        Type = accessKeyType,
        Expires = DateTimeOffset.Now.AddMinutes(5),
        TryCount = 0
      });
      _context.Add(new AccessKey
      {
        Email = "Entity@nhs.co.uk",
        Key = "123452",
        Type = accessKeyType,
        Expires = DateTimeOffset.Now.AddMinutes(2),
        TryCount = 0
      });
      await _context.SaveChangesAsync();

      ValidateAccessKey validateAccessKey = new()
      {
        AccessKey = key,
        Email = "Entity@nhs.co.uk",
        Type = accessKeyType
      };

      // Act.
      IValidateAccessKeyResponse response =
        await _service.ValidateAccessKeyAsync(validateAccessKey);

      // Assert.
      using (new AssertionScope())
      {
        response.Should().BeOfType<ValidateAccessKeyResponse>();
        response.ErrorType.Should().Be(ErrorTypes.None);
        response.HasErrors.Should().BeFalse();
        response.IsValidCode.Should().BeTrue();
      }

      CleanUp();
    }
  }
}
