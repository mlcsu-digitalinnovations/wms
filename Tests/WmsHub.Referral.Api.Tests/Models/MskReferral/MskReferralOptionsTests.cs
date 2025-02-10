using FluentAssertions;
using Moq;
using System;
using WmsHub.Common.Exceptions;
using WmsHub.Referral.Api.Models.MskReferral;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Referral.Api.Tests.Models.MskReferral;

public class MskReferralOptionsTests : AModelsBaseTests
{

  public class HasEmailDomainWhiteListTests : MskReferralOptionsTests
  {
    [Fact]
    public void EmailDomainWhiteListIsNull_False()
    {
      // Arrange.
      MskReferralOptions options = new()
      {
        EmailDomainWhitelist = null
      };

      // Act.
      bool result = options.HasEmailDomainWhiteList;

      // Assert.
      result.Should().BeFalse();
    }

    [Fact]
    public void EmailDomainWhiteListHasLengthZero_False()
    {
      // Arrange.
      MskReferralOptions options = new()
      {
        EmailDomainWhitelist = System.Array.Empty<string>()
      };

      // Act. 
      bool result = options.HasEmailDomainWhiteList;

      // Assert.
      result.Should().BeFalse();
    }

    [Fact]
    public void EmailDomainWhiteListHasEntries_True()
    {
      // Arrange.
      MskReferralOptions options = new()
      {
        EmailDomainWhitelist = new string[] { "nhs.net" }
      };

      // Act. 
      bool result = options.HasEmailDomainWhiteList;

      // Assert.
      result.Should().BeTrue();

    }

  }

  public class IsEmailInWhitelistTests : MskReferralOptionsTests
  {
    [Theory]
    [MemberData(nameof(NullOrWhiteSpaceTheoryData))]
    public void EmailNullOrWhiteSpace_Exception(string email)
    {
      // Arrange.
      MskReferralOptions options = new();

      // Act.
      Exception ex = Record.Exception(
        () => options.IsEmailDomainInWhitelist(email));

      // Assert.
      ex.Should().BeOfType<ArgumentNullOrWhiteSpaceException>();
    }

    [Fact]
    public void IsEmailDomainWhitelistEnabledIsFalse_True()
    {
      // Arrange.
      MskReferralOptions options = new()
      {
        IsEmailDomainWhitelistEnabled = false
      };

      // Act.
      bool result = options.IsEmailDomainInWhitelist("test@nhs.net");

      // Assert.
      result.Should().BeTrue();
    }

    [Fact]
    public void HasEmailDomainWhiteListIsFalse_Exception()
    {
      // Arrange.
      Mock<MskReferralOptions> mockOptions = new();
      mockOptions.CallBase = true;
      mockOptions.Setup(x => x.IsEmailDomainWhitelistEnabled).Returns(true);
      mockOptions.Setup(x => x.HasEmailDomainWhiteList).Returns(false);

      // Act.
      Exception ex = Record.Exception(
        () => mockOptions.Object.IsEmailDomainInWhitelist("test@nhs.net"));

      // Assert.
      ex.Should().BeOfType<EmailWhiteListException>();
    }

    [Fact]
    public void EmailNotInEmailDomainWhiteList_False()
    {
      // Arrange.
      Mock<MskReferralOptions> mockOptions = new();
      mockOptions.CallBase = true;
      mockOptions.Setup(x => x.IsEmailDomainWhitelistEnabled).Returns(true);
      mockOptions.Setup(x => x.HasEmailDomainWhiteList).Returns(true);
      mockOptions.Object.EmailDomainWhitelist 
        = new string[] { "nhs.uk", "test.nhs.uk" };

      // Act.
      bool result = mockOptions.Object.IsEmailDomainInWhitelist("test@nhs.net");

      // Assert.
      result.Should().BeFalse();
    }

    [Fact]
    public void EmailInEmailDomainWhiteList_True()
    {
      // Arrange.
      Mock<MskReferralOptions> mockOptions = new();
      mockOptions.CallBase = true;
      mockOptions.Setup(x => x.IsEmailDomainWhitelistEnabled).Returns(true);
      mockOptions.Setup(x => x.HasEmailDomainWhiteList).Returns(true);
      mockOptions.Object.EmailDomainWhitelist
        = new string[] { "nhs.uk", "nhs.net" };

      // Act.
      bool result = mockOptions.Object.IsEmailDomainInWhitelist("test@nhs.uk");

      // Assert.
      result.Should().BeTrue();
    }

  }
}
