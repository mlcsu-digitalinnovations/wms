using FluentAssertions;
using System.Threading.Tasks;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Helpers;
public class TelephoneHelperTests : ATheoryData
{
  public class FixPhoneNumberFields : TelephoneHelperTests
  {
    public class MockPhoneBase : IPhoneBase
    {
      public string Mobile { get; set; }
      public string Telephone { get; set; }
      public bool? IsMobileValid { get; set; }
      public bool? IsTelephoneValid { get; set; }
    }

    [Theory]
    [MemberData(nameof(MobileInvalidTelephoneInvalidData))]
    public void MobileInvalid_TelephoneInvalid(
      string mobile,
      string telephone)
    {
      // Arrange.
      MockPhoneBase mockPhoneBase = new()
      {
        Mobile = mobile,
        Telephone = telephone
      };
      string expectedMobile = mobile == "" ? null : mobile;
      string expectedTelephone = telephone == "" ? null : telephone;

      // Act.
      mockPhoneBase.FixPhoneNumberFields();

      // Assert.
      mockPhoneBase.Mobile.Should().Be(expectedMobile);
      mockPhoneBase.IsMobileValid.Should().BeFalse();
      mockPhoneBase.Telephone.Should().Be(expectedTelephone);
      mockPhoneBase.IsTelephoneValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(MOBILE_E164, TELEPHONE_E164)]
    [InlineData(MOBILE, TELEPHONE)]
    public void MobileIsMobile_TelephoneIsTelephone(
      string mobile,
      string telephone)
    {
      // Arrange.
      MockPhoneBase mockPhoneBase = new()
      {
        Mobile = mobile,
        Telephone = telephone
      };

      // Act.
      mockPhoneBase.FixPhoneNumberFields();

      // Assert.
      mockPhoneBase.Mobile.Should().Be(MOBILE_E164);
      mockPhoneBase.IsMobileValid.Should().BeTrue();
      mockPhoneBase.Telephone.Should().Be(TELEPHONE_E164);
      mockPhoneBase.IsTelephoneValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(TELEPHONE_E164, MOBILE_E164)]
    [InlineData(TELEPHONE, MOBILE)]
    public void MobileIsTelephone_TelephoneIsMobile(
      string mobile,
      string telephone)
    {
      // Arrange.
      MockPhoneBase mockPhoneBase = new()
      {
        Mobile = mobile,
        Telephone = telephone
      };

      // Act.
      mockPhoneBase.FixPhoneNumberFields();

      // Assert.
      mockPhoneBase.Mobile.Should().Be(MOBILE_E164);
      mockPhoneBase.IsMobileValid.Should().BeTrue();
      mockPhoneBase.Telephone.Should().Be(TELEPHONE_E164);
      mockPhoneBase.IsTelephoneValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(MobileInvalidTelephoneIsMobileData))]
    public void MobileInvalid_TelephoneIsMobile(
      string mobile,
      string telephone)
    {
      // Arrange.
      MockPhoneBase mockPhoneBase = new()
      {
        Mobile = mobile,
        Telephone = telephone
      };

      // Act.
      mockPhoneBase.FixPhoneNumberFields();

      // Assert.
      mockPhoneBase.Mobile.Should().Be(MOBILE_E164);
      mockPhoneBase.IsMobileValid.Should().BeTrue();
      mockPhoneBase.Telephone.Should().BeNull();
      mockPhoneBase.IsTelephoneValid.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(MobileIsTelephoneTelephoneInvalidData))]
    public void MobileIsTelephone_TelephoneInvalid(
      string mobile,
      string telephone)
    {
      // Arrange.
      MockPhoneBase mockPhoneBase = new()
      {
        Mobile = mobile,
        Telephone = telephone
      };

      // Act.
      mockPhoneBase.FixPhoneNumberFields();

      // Assert.
      mockPhoneBase.Mobile.Should().BeNull();
      mockPhoneBase.IsMobileValid.Should().BeFalse();
      mockPhoneBase.Telephone.Should().Be(TELEPHONE_E164);
      mockPhoneBase.IsTelephoneValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(MobileInvalidTelephoneIsTelephoneData))]
    public void MobileInvalid_TelephoneIsTelephone(
      string mobile,
      string telephone)
    {
      // Arrange.
      MockPhoneBase mockPhoneBase = new()
      {
        Mobile = mobile,
        Telephone = telephone
      };
      string expectedMobile = mobile == "" ? null : mobile;

      // Act.
      mockPhoneBase.FixPhoneNumberFields();

      // Assert.
      mockPhoneBase.Mobile.Should().Be(expectedMobile);
      mockPhoneBase.IsMobileValid.Should().BeFalse();
      mockPhoneBase.Telephone.Should().Be(TELEPHONE_E164);
      mockPhoneBase.IsTelephoneValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(MobileIsMobileTelephoneInvalidData))]
    public void MobileIsMobile_TelephoneInvalid(
      string mobile,
      string telephone)
    {
      // Arrange.
      MockPhoneBase mockPhoneBase = new()
      {
        Mobile = mobile,
        Telephone = telephone
      };
      string expectedTelephone = telephone == "" ? null : telephone;

      // Act.
      mockPhoneBase.FixPhoneNumberFields();

      // Assert.
      mockPhoneBase.Mobile.Should().Be(MOBILE_E164);
      mockPhoneBase.IsMobileValid.Should().BeTrue();
      mockPhoneBase.Telephone.Should().Be(expectedTelephone);
      mockPhoneBase.IsTelephoneValid.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(MobileIsMobileTelephoneIsMobile))]
    public void MobileIsMobile_TelephoneIsMobile(
      string mobile,
      string telephone)
    {
      // Arrange.
      MockPhoneBase mockPhoneBase = new()
      {
        Mobile = mobile,
        Telephone = telephone
      };

      // Act.
      mockPhoneBase.FixPhoneNumberFields();

      // Assert.
      mockPhoneBase.Mobile.Should().Be(MOBILE_E164);
      mockPhoneBase.IsMobileValid.Should().BeTrue();
      mockPhoneBase.Telephone.Should().BeNull();
      mockPhoneBase.IsTelephoneValid.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(MobileIsTelephoneTelephoneIsTelephone))]
    public void MobileIsTelephone_TelephoneIsTelephone(
      string mobile,
      string telephone)
    {
      // Arrange.
      MockPhoneBase mockPhoneBase = new()
      {
        Mobile = mobile,
        Telephone = telephone
      };

      // Act.
      mockPhoneBase.FixPhoneNumberFields();

      // Assert.
      mockPhoneBase.Mobile.Should().BeNull();
      mockPhoneBase.IsMobileValid.Should().BeFalse();
      mockPhoneBase.Telephone.Should().Be(TELEPHONE_E164);
      mockPhoneBase.IsTelephoneValid.Should().BeTrue();
    }
  }
}
