using FluentAssertions;
using System;
using WmsHub.Business.Models.ChatBotService;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
  public class UpdateReferralWithCallRequestTests : AModelsBaseTests
  {
    [Fact]
    public void Valid()
    {
      // arrange
      UpdateReferralWithCallRequest model = Create();

      // act
      ValidateModelResult result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeTrue();
      result.Results.Should().BeEmpty();
    }

    [Fact]
    public void Id_Invalid()
    {
      // arrange
      UpdateReferralWithCallRequest model = Create();
      model.Id = default;

      string expectedErrorMessage = new InvalidValidationResult(
        nameof(model.Id), model.Id)
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
      UpdateReferralWithCallRequest model = Create();
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
      UpdateReferralWithCallRequest model = Create();
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
      UpdateReferralWithCallRequest model = Create();
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

    [Fact]
    public void Outcome_Invalid()
    {
      // arrange
      UpdateReferralWithCallRequest model = Create();
      model.Outcome = "UnknownOutcome";

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
    [InlineData("VoicemailLeft")]
    [InlineData("Connected")]
    [InlineData("HungUp")]
    [InlineData("Engaged")]
    [InlineData("CallGuardian")]
    [InlineData("NoAnswer")]
    [InlineData("InvalidNumber")]
    [InlineData("Error")]
    [Theory]
    public void Outcome_Valid(string outcome)
    {
      // arrange
      UpdateReferralWithCallRequest model = Create();
      model.Outcome = outcome;

      // act
      ValidateModelResult result = ValidateModel(model);

      // assert
      result.IsValid.Should().BeTrue();
      result.Results.Should().BeEmpty();
    }


    [Fact]
    public void Timestamp_Invalid()
    {
      // arrange
      UpdateReferralWithCallRequest model = Create();
      model.Timestamp = default;

      string expectedErrorMessage = new InvalidValidationResult(
        nameof(model.Timestamp), model.Timestamp)
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
      UpdateReferralWithCallRequest model = Create();
      model.Timestamp =
        DateTimeOffset.Now
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

    private static UpdateReferralWithCallRequest Create(
      Guid id = default,
      string outcome = null,
      string number = "+441743123456",
      DateTimeOffset timestamp = default)
    {
      return new UpdateReferralWithCallRequest(
        id: id == default ? Guid.NewGuid() : id,
        outcome: outcome ?? Enums.ChatBotCallOutcome.CallerReached.ToString(),
        number: number,
        timestamp: timestamp == default ? DateTimeOffset.Now : timestamp);
    }
  }
}
