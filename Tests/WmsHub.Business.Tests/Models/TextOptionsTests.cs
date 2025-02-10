using FluentAssertions;
using System.Linq;
using WmsHub.Business.Models.Notify;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class TextOptionsTests : AModelsBaseTests
  {
    public class SmsApiKey : TextOptionsTests
    {
      [Fact]
      public void StringIsValidateModelInvalid()
      {
        //Arrange
        var expected = "The SmsApiKey field is required.";
        var textOptions = new TextOptions();

        //Act
        ValidateModelResult result = ValidateModel(textOptions);

        //Assert
        result.IsValid.Should().BeFalse();
        result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
      }

      [Fact]
      public void StringIsValid()
      {
        //Arrange
        var key = "test key";
        var settings = new TextOptions { SmsApiKey = key };

        //act & Assert
        Assert.Equal(key, settings.SmsApiKey);
      }
    }

    public class SmsSenderId : TextOptionsTests
    {
      [Fact]
      public void StringIsEmptyValidateModelInvalid()
      {
        //Arrange
        var expected = "The SmsSenderId field is required.";
        var textOptions = new TextOptions();

        //Act
        ValidateModelResult result = ValidateModel(textOptions);

        //Assert
        result.IsValid.Should().BeFalse();
        result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
      }

      [Fact]
      public void StringIsValid()
      {
        //Arrange
        var senderId = "test id";
        var settings = new TextOptions { SmsSenderId = senderId };
        //act & Assert
        Assert.Equal(senderId, settings.SmsSenderId);
      }
    }

    public class TokenSecret : TextOptionsTests
    {
      [Fact]
      public void StringIsEmptyValidateModelInvalid()
      {
        // Arrange
        var textOptions = new TextOptions();
        var expected = "The TokenSecret field is required.";

        //Act
        ValidateModelResult result = ValidateModel(textOptions);

        //Assert
        result.IsValid.Should().BeFalse();
        result.Results.Select(r => r.ErrorMessage).Should().Contain(expected);
      }

      [Fact]
      public void StringIsValid()
      {
        //Arrange
        var secret = "test id";
        var settings = new TextOptions { TokenSecret = secret };
        //act & Assert
        Assert.Equal(secret, settings.TokenSecret);
      }
    }
  }
}