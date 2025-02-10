using FluentAssertions;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Common.Extensions;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models.GpDocumentProxy;
public class GpDocumentProxyReferralDischargeTests : AModelsBaseTests
{

  [Theory]
  [MemberData(nameof(ReferralSourceTheoryData))]
  public void SetReferralSourceSetReferralSourceDescription(ReferralSource referralSource)
  {
    // Arrange.
    GpDocumentProxyReferralDischarge discharge = new();
    string expectedReferralSourceDescription = referralSource.GetDescriptionAttributeValue();

    // Act.
    discharge.ReferralSource = referralSource.ToString();

    // Assert.
    discharge.ReferralSourceDescription.Should().Be(expectedReferralSourceDescription);
  }
}
