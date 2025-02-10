using FluentAssertions;
using System;
using WmsHub.Business.Enums;
using WmsHub.ChatBot.Api.Models;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.ChatBot.Api.Tests;

public class ReferralCallTests : AModelsBaseTests
{
  [Fact]
  public void Valid()
  {
    // Arrange.
    ReferralCall model = Create();

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.IsValid.Should().BeTrue();
    result.Results.Should().BeEmpty();
  }

  [Fact]
  public void Id_Required()
  {
    // Arrange.
    ReferralCall model = Create();
    model.Id = default;

    string expectedErrorMessage = "The Id field is required.";

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Should().HaveCount(1);
    result.GetErrorMessage().Should().Be(expectedErrorMessage);
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  public void Number_Required(string number)
  {
    // Arrange.
    ReferralCall model = Create();
    model.Number = number;

    string expectedErrorMessage = "The Number field is required.";

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
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
    // Arrange.
    ReferralCall model = Create();
    model.Number = number;

    string expectedErrorMessage = "The field Number is not a valid telephone number.";

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Should().HaveCount(1);
    result.GetErrorMessage().Should().Be(expectedErrorMessage);
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  public void Outcome_Required(string outcome)
  {
    // Arrange.
    ReferralCall model = Create();
    model.Outcome = outcome;

    string expectedErrorMessage = "The Outcome field is required.";

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
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
    // Arrange.
    ReferralCall model = Create();
    model.Outcome = outcome;

    string expectedErrorMessage = $"The Outcome field '{outcome}' is invalid.";

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
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
    // Arrange.
    ReferralCall model = Create();
    model.Outcome = outcome;

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.IsValid.Should().BeTrue();
    result.Results.Should().BeEmpty();
  }

  [Fact]
  public void Timestamp_Required()
  {
    // Arrange.
    ReferralCall model = Create();
    model.Timestamp = default;

    string expectedErrorMessage = "The Timestamp field is required.";

    // Act.
    ValidateModelResult result = ValidateModel(model);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Should().HaveCount(1);
    result.GetErrorMessage().Should().Be(expectedErrorMessage);
  }

  [Fact]
  public void Timestamp_MaxMinutesAhead()
  {
    // Arrange.
    ReferralCall referralCall = Create();
    referralCall.Timestamp = DateTimeOffset.Now
      .AddMinutes(Constants.MAX_SECONDS_API_REQUEST_AHEAD + 1);

    string expectedErrorMessage =
      $"*Timestamp is more than {Constants.MAX_SECONDS_API_REQUEST_AHEAD} seconds(s) ahead*.";

    // Act.
    ValidateModelResult result = ValidateModel(referralCall);

    // Assert.
    result.IsValid.Should().BeFalse();
    result.Results.Should().HaveCount(1);
    result.GetErrorMessage().Should().Match(expectedErrorMessage);
  }

  private static ReferralCall Create(
    Guid id = default,
    string outcome = null,
    string number = "+441743123456",
    DateTimeOffset timestamp = default)
  {
    ReferralCall referralCall = new()
    {
      Id = id == default ? Guid.NewGuid() : id,
      Outcome = outcome ?? ChatBotCallOutcome.CallerReached.ToString(),
      Number = number,
      Timestamp = timestamp == default ? DateTimeOffset.Now : timestamp
    };
    return referralCall;
  }
}
