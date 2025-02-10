using FluentAssertions;
using System.Text.Json;
using WmsHub.Business.Models;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class ServiceUserTest : AModelsBaseTests
  {
    public class JsonIgnoreField : ServiceUserTest
    {
      [Fact]
      public void IgnoreIsMoblieValid()
      {
        //Arrange
        string UKMobile = "+447708342934";
        bool? execptedIsMobileValid = true;

        var serviceUser = new ServiceUser
        {
          Mobile = UKMobile,
          IsMobileValid = execptedIsMobileValid
        };
        
        //Act
        var jsonString = JsonSerializer.Serialize(serviceUser);
        
        //Assert
        var jDoc = JsonDocument.Parse(jsonString);
        jDoc.RootElement.TryGetProperty("IsMobileValid",
          out var isMobileValidValue).Should().BeFalse();
        jDoc.RootElement.TryGetProperty("Mobile",
          out var MobileValue).Should().BeTrue();
        MobileValue.GetString().Should().Be(UKMobile);
      }

      [Fact]
      public void IgnoreIsTelephoneValid()
      {
        //Arrange
        string UKTelephone = "+441782872500";
        bool? execptedIsTelephoneValid = true;

        var serviceUser = new ServiceUser
        {
          Telephone = UKTelephone,
          IsTelephoneValid = execptedIsTelephoneValid
        };

        //Act
        var jsonString = JsonSerializer.Serialize(serviceUser);
        
        //Assert
        var jDoc = JsonDocument.Parse(jsonString);
        jDoc.RootElement.TryGetProperty("IsTelephoneValid",
          out var isTelephoneValidValue).Should().BeFalse();
        jDoc.RootElement.TryGetProperty("Telephone",
          out var TelephoneValue).Should().BeTrue();
        TelephoneValue.GetString().Should().Be(UKTelephone);
      }
    }
    public class Telephone : ServiceUserTest
    {
      [Fact]
      public void UkTelephone_IsTelephoneValidNull()
      {
        //Arrange
        string expectedUkTelephone = "+441782872500";
        bool? expectedIsTelephoneValid = null;

        var serviceUser = new ServiceUser
        {
          Telephone = expectedUkTelephone,
          IsTelephoneValid = expectedIsTelephoneValid
        };

        //Act & Assert
        serviceUser.Telephone.Should().Be(expectedUkTelephone);
        serviceUser.IsTelephoneValid.Should().BeNull();
      }

      [Fact]
      public void UkTelephone_IsTelephoneValidTrue()
      {
        //Arrange
        string expectedUkTelephone = "+441782872500";
        bool? expectedIsTelephoneValid = true;

        var serviceUser = new ServiceUser
        {
          Telephone = expectedUkTelephone,
          IsTelephoneValid = expectedIsTelephoneValid
        };

        //Act & Assert
        serviceUser.Telephone.Should().Be(expectedUkTelephone);
        serviceUser.IsTelephoneValid.Should().BeTrue();
      }

      [Fact]
      public void NotUkTelephone_IsTelephoneValidFalse()
      {
        //Arrange
        string expectedNotUkTelephone = "+85226913430";
        bool? expectedIsTelephoneValid = false;

        var serviceUser = new ServiceUser
        {
          Telephone = expectedNotUkTelephone,
          IsTelephoneValid = expectedIsTelephoneValid
        };

        //Act & Assert
        serviceUser.Telephone.Should().BeNull();
        serviceUser.IsTelephoneValid.Should().BeFalse();
      }

      [Fact]
      public void EmptyTelephone_IsTelephoneValidFalse()
      {
        //Arrange
        string expectedEmptyTelephone = "";
        bool? expectedIsTelephoneValid = false;

        var serviceUser = new ServiceUser
        {
          Telephone = expectedEmptyTelephone,
          IsTelephoneValid = expectedIsTelephoneValid
        };

        //Act & Assert
        serviceUser.Telephone.Should().BeNull();
        serviceUser.IsTelephoneValid.Should().BeFalse();
      }
    }
    public class Mobile : ServiceUserTest
    {
      [Fact]
      public void UkMobile_IsMobileValidNull()
      {
        //Arrange
        string expectedUkMobile = "+447708342934";
        bool? expectedIsMobileValid = null;

        var serviceUser = new ServiceUser
        {
          Mobile = expectedUkMobile,
          IsMobileValid = expectedIsMobileValid
        };

        //Act & Assert
        serviceUser.Mobile.Should().Be(expectedUkMobile);
        serviceUser.IsMobileValid.Should().BeNull();
      }

      [Fact]
      public void UkMobile_IsMobileValidTrue()
      {
        //Arrange
        string expectedUkMobile = "+447708342934";
        bool? expectedIsMobileValid = true;

        var serviceUser = new ServiceUser
        {
          Mobile = expectedUkMobile,
          IsMobileValid = expectedIsMobileValid
        };

        //Act & Assert
        serviceUser.Mobile.Should().Be(expectedUkMobile);
        serviceUser.IsMobileValid.Should().BeTrue();
      }

      [Fact]
      public void NotUkMobile_IsMobileValidFalse()
      {
        //Arrange
        string expectedNotUkMobile = "+85265183024";
        bool? expectedIsMobileValid = false;

        var serviceUser = new ServiceUser
        {
          Mobile = expectedNotUkMobile,
          IsMobileValid = expectedIsMobileValid
        };

        //Act & Assert
        serviceUser.Mobile.Should().BeNull();
        serviceUser.IsMobileValid.Should().BeFalse();
      }

      [Fact]
      public void EmptyMobile_IsMobileValidFalse()
      {
        //Arrange
        string expectedEmptyMobile = "";
        bool? expectedIsMobileValid = false;

        var serviceUser = new ServiceUser
        {
          Mobile = expectedEmptyMobile,
          IsMobileValid = expectedIsMobileValid
        };

        //Act & Assert
        serviceUser.Mobile.Should().BeNull();
        serviceUser.IsMobileValid.Should().BeFalse();
      }
    }
  }
}
