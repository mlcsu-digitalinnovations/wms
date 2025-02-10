using FluentAssertions;
using System;
using System.Threading.Tasks;
using UglyToad.PdfPig.Core;
using WmsHub.Business.Enums;
using WmsHub.Common.Extensions;
using Xunit;

namespace WmsHub.Common.Tests.Extensions;
public class ReferralStatusTraceAttributeExtensionTests()
{
  public class CanTraceReferralStatusTests() : ReferralStatusTraceAttributeExtensionTests
  {
    [Fact]
    public void InvalidReferralStatusValueThrowsException()
    {
      // Arrange.
      long invalidReferralEnumIndex = 3;
      ReferralStatus referralStatus = (ReferralStatus)invalidReferralEnumIndex;

      // Act.
      Func<bool> result = () => referralStatus.CanTraceReferralStatus();

      // Assert.
      result.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ReferralStatusWithAttributeReturnsTrue()
    {
      // Arrange.

      // Act.
      bool canTrace = ReferralStatus.New.CanTraceReferralStatus();

      // Assert.
      canTrace.Should().BeTrue();
    }

    [Fact]
    public void ReferralStatusWithoutAttributeReturnsFalse()
    {
      // Arrange.

      // Act.
      bool canTrace = ReferralStatus.Complete.CanTraceReferralStatus();

      // Assert.
      canTrace.Should().BeFalse();
    }
  }
  public class CanTraceReferralStatusStringTests() : ReferralStatusTraceAttributeExtensionTests
  {
    [Fact]
    public void InvalidReferralStatusStringReturnsFalse()
    {
      // Arrange.
      string invalidReferralStatusString = "invalid string";

      // Act.
      bool canTrace = invalidReferralStatusString.CanTraceReferralStatusString<ReferralStatus>();

      // Assert.
      canTrace.Should().BeFalse();
    }

    [Fact]
    public void ReferralStatusStringWithAttributeReturnsTrue()
    {
      // Arrange.
      string validStatusWithAttributeString = ReferralStatus.New.ToString();

      // Act.
      bool canTrace = validStatusWithAttributeString.CanTraceReferralStatusString<ReferralStatus>();

      // Assert.
      canTrace.Should().BeTrue();
    }

    [Fact]
    public void ReferralStatusStringWithoutAttributeReturnsFalse()
    {
      // Arrange.
      string validStatusWithoutAttributeString = ReferralStatus.Complete.ToString();
      // Act.
      bool canTrace = 
        validStatusWithoutAttributeString.CanTraceReferralStatusString<ReferralStatus>();

      // Assert.
      canTrace.Should().BeFalse();
    }
  }
}
