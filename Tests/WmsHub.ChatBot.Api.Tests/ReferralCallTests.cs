using FluentAssertions;
using System;
using WmsHub.Business.Enums;
using WmsHub.ChatBot.Api.Models;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.ChatBot.Api.Tests.Models
{
  public class ReferralCallTests : AModelsBaseTests
  {
    [Fact]
    public void Valid()
    {
      // arrange
      ReferralCall model = Create();

      // act
      ValidateModelResult result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeTrue();
      result.Results.Should().BeEmpty();
    }

    [Fact]
    public void Id_Required()
    {
      // arrange
      ReferralCall model = Create();
      model.Id = default;

      string expectedErrorMessage = 
        new RequiredValidationResult(nameof(model.Id))
        .ErrorMessage;

      // act
      ValidateModelResult result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.GetErrorMessage().Should().Be(expectedErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Number_Required(string number)
    {
      // arrange
      ReferralCall model = Create();
      model.Number = number;

      string expectedErrorMessage = new RequiredValidationResult(
        nameof(model.Number))
          .ErrorMessage;

      // act
      ValidateModelResult result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.GetErrorMessage().Should().Be(expectedErrorMessage);
    }

    [InlineData("01743123456")]
    [InlineData("0+1743123456")]
    [InlineData("01743123456+")]
    [InlineData("01743one23456+")]
    [Theory]
    public void Number_RegularExpression(string number)
    {
      // arrange
      ReferralCall model = Create();
      model.Number = number;

      string expectedErrorMessage = 
        "The field Number is not a valid telephone number.";

      // act
      ValidateModelResult result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.GetErrorMessage().Should().Be(expectedErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Outcome_Required(string outcome)
    {
      // arrange
      ReferralCall model = Create();
      model.Outcome = outcome;

      string expectedErrorMessage = new RequiredValidationResult(
        nameof(model.Outcome))
          .ErrorMessage;

      // act
      ValidateModelResult result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.GetErrorMessage().Should().Be(expectedErrorMessage);
    }

    [InlineData("T")]
    [InlineData("Te")]
    [InlineData("Unknown")]
    [Theory]
    public void Outcome_Invalid(string outcome)
    {
      // arrange
      ReferralCall model = Create();
      model.Outcome = outcome;

      string expectedErrorMessage = new InvalidValidationResult(
        nameof(model.Outcome), model.Outcome)
          .ErrorMessage;

      // act
      ValidateModelResult result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.GetErrorMessage().Should().Be(expectedErrorMessage);
    }

    [InlineData("CallerReached")]
    [InlineData("TransferredToPhoneNumber")]
    [InlineData("TransferredToQueue")]    
    [InlineData("TransferredToVoicemail")]
    [InlineData("TransferringToRmc")]
    [InlineData("VoicemailLeft")]
    [InlineData("Connected")]
    [InlineData("HungUp")]
    [InlineData("Engaged")]
    [InlineData("CallGuardian")]
    [InlineData("NoAnswer")]
    [InlineData("InvalidNumber")]
    [InlineData("Error")]
    [InlineData("callerReached")]
    [InlineData("transferredToPhoneNumber")]
    [InlineData("transferredToQueue")]
    [InlineData("transferredToVoicemail")]
    [InlineData("transferringToRmc")]
    [InlineData("voicemailLeft")]
    [InlineData("connected")]
    [InlineData("hungUp")]
    [InlineData("engaged")]
    [InlineData("callGuardian")]
    [InlineData("noAnswer")]
    [InlineData("invalidNumber")]
    [InlineData("error")]
    [Theory]
    public void Outcome_Valid(string outcome)
    {
      // arrange
      ReferralCall model = Create();
      model.Outcome = outcome;

      // act
      ValidateModelResult result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeTrue();
      result.Results.Should().BeEmpty();
    }


    [Fact]
    public void Timestamp_Required()
    {
      // arrange
      ReferralCall model = Create();
      model.Timestamp = default;

      string expectedErrorMessage = new RequiredValidationResult(
        nameof(model.Timestamp))
          .ErrorMessage;

      // act
      ValidateModelResult result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.GetErrorMessage().Should().Be(expectedErrorMessage);
    }

    [Fact]
    public void Timestamp_MaxMinutesAhead()
    {
      // arrange
      ReferralCall model = Create();
      model.Timestamp = DateTimeOffset.Now
        .AddMinutes(Constants.MAX_SECONDS_API_REQUEST_AHEAD + 1);

      string expectedErrorMessage = new MaxSecondsAheadValidationResult(
        nameof(model.Timestamp), Constants.MAX_SECONDS_API_REQUEST_AHEAD)
          .ErrorMessage;

      // act
      ValidateModelResult result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeFalse();
      result.Results.Should().HaveCount(1);
      result.GetErrorMessage().Should().Be(expectedErrorMessage);
    }

    private static ReferralCall Create(
      Guid id = default,
      string outcome = null,
      string number = "+441743123456",
      DateTimeOffset timestamp = default)
    {
      ReferralCall referralCall = new ReferralCall()
      {
        Id = id == default ? Guid.NewGuid() : id,
        Outcome = outcome ?? ChatBotCallOutcome.CallerReached.ToString(),
        Number = number,
        Timestamp = timestamp == default ? DateTimeOffset.Now : timestamp
      };
      return referralCall;
    }
  }
}
