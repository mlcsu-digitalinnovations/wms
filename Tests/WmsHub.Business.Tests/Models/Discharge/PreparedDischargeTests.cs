using FluentAssertions;
using System;
using System.Collections.Generic;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.Discharge;
using WmsHub.Business.Models.ProviderService;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models.Discharge
{
  public class PreparedDischargeTests : AModelsBaseTests
  {
    const int DISCHARGE_AFTER_DAYS = 94;
    const int DISCHARGE_COMPLETION_DAYS = 49;
    private const int TERMINATEAFTERDAYS = 42;
    const int WEIGHT_CHANGE_THRESHOLD = 25;

    public PreparedDischargeTests()
    {
      ProviderOptions providerOptions = new()
      {
        DischargeAfterDays = DISCHARGE_AFTER_DAYS,
        DischargeCompletionDays = DISCHARGE_COMPLETION_DAYS,
        WeightChangeThreshold = WEIGHT_CHANGE_THRESHOLD
      };
      ReferralTimelineOptions referralTimelineOptions = new()
      {
        MaxDaysToStartProgrammeAfterProviderSelection = TERMINATEAFTERDAYS
      };

      PreparedDischarge.SetOptions(providerOptions, referralTimelineOptions);
    }

    [Fact]
    public void DateCompletedProgrammeDefaultDateTimeOffset_Exception()
    {
      // Arrange.
      DateTimeOffset dateCompletedProgramme = default;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now;
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;
      string expectedExceptionMessage = "*default*dateCompletedProgramme*";
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;
      List<Entities.ProviderSubmission> submissions = [];

      // Act.
      Action act = () => _ = new PreparedDischarge(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      act.Should().Throw<ArgumentException>()
        .Which.Message.Should().Match(expectedExceptionMessage);
    }

    [Fact]
    public void DateOfProviderSelectionDefaultDateTimeOffsetException()
    {
      // Arrange.
      
      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateOfProviderSelection = default;
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;
      string expectedExceptionMessage = "*default*dateOfProviderSelection*";
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;
      List<Entities.ProviderSubmission> submissions = [];

      // Act.
      Action act = () => _ = new PreparedDischarge(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      act.Should().Throw<ArgumentException>()
        .Which.Message.Should().Match(expectedExceptionMessage);
    }

    [Fact]
    public void DateStartedProgrammeDefaultDateTimeOffset_Exception()
    {
      // Arrange.
      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now;
      DateTimeOffset dateStartedProgramme = default;
      string expectedExceptionMessage = "*default*dateStartedProgramme*";
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;
      List<Entities.ProviderSubmission> submissions = [];

      // Act.
      Action act = () => _ = new PreparedDischarge(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      act.Should().Throw<ArgumentException>()
        .Which.Message.Should().Match(expectedExceptionMessage);
    }

    [Fact]
    public void DateStartedProgrammeNullDidNotCommence()
    {
      // Arrange.
      DateTimeOffset? dateStartedProgramme = null;
      DateTimeOffset? dateCompletedProgramme = null;
      DateTimeOffset expectedDateOfLastEngagement = DateTimeOffset.UtcNow;
      DateTimeOffset dateOfProviderSelection = expectedDateOfLastEngagement
        .AddDays(-TERMINATEAFTERDAYS);
      string expectedProgrammeOutcome = ProgrammeOutcome.DidNotCommence.ToString();
      string expectedStatus = ReferralStatus.AwaitingDischarge.ToString();
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;
      List<Entities.ProviderSubmission> submissions = new();

      // Act.
      PreparedDischarge preparedDischarge = new(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      preparedDischarge.DateOfLastEngagement.Should().Be(expectedDateOfLastEngagement);
      preparedDischarge.DateStartedProgramme.Should().Be(dateStartedProgramme);
      preparedDischarge.FirstRecordedWeight.Should().BeNull();
      preparedDischarge.FirstRecordedWeightDate.Should().BeNull();
      preparedDischarge.FirstWeightSubmission.Should().BeNull();
      preparedDischarge.Id.Should().Be(id);
      preparedDischarge.IsAwaitingDischarge.Should().BeTrue();
      preparedDischarge.IsDischargeOnHold.Should().BeFalse();
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeFalse();
      preparedDischarge.IsUnableToDischarge.Should().BeFalse();
      preparedDischarge.LastRecordedWeight.Should().BeNull();
      preparedDischarge.LastRecordedWeightDate.Should().BeNull();
      preparedDischarge.LastWeightSubmission.Should().BeNull();
      preparedDischarge.NhsNumber.Should().Be(nhsNumber);
      preparedDischarge.ProgrammeOutcome.Should().Be(expectedProgrammeOutcome);
      preparedDischarge.ReferringGpPracticeNumber.Should().Be(referringGpPracticeNumber);
      preparedDischarge.Status.Should().Be(expectedStatus);
      preparedDischarge.StatusReason.Should().BeNull();
      preparedDischarge.WeightChange.Should().Be(0);
    }

    [Fact]
    public void DischargeAfterDaysNotSet_Exception()
    {
      // Arrange.
      ProviderOptions providerOptions = new()
      {
        DischargeAfterDays = PreparedDischarge.NOT_SET,
        DischargeCompletionDays = DISCHARGE_COMPLETION_DAYS,
        WeightChangeThreshold = WEIGHT_CHANGE_THRESHOLD
      };
      ReferralTimelineOptions referralTimelineOptions = new()
      {
        MaxDaysToStartProgrammeAfterProviderSelection = TERMINATEAFTERDAYS
      };
      PreparedDischarge.SetOptions(providerOptions, referralTimelineOptions);

      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now;
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;
      string expectedExceptionMessage = "DischargeAfterDays";
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;
      List<Entities.ProviderSubmission> submissions = [];

      // Act.
      Action act = () => _ = new PreparedDischarge(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      act.Should().Throw<InvalidOperationException>()
        .Which.Message.Should().Contain(expectedExceptionMessage);
    }

    [Fact]
    public void DischargeCompletionDaysNotSet_Exception()
    {
      // Arrange.
      ProviderOptions providerOptions = new()
      {
        DischargeAfterDays = DISCHARGE_AFTER_DAYS,
        DischargeCompletionDays = PreparedDischarge.NOT_SET,
        WeightChangeThreshold = WEIGHT_CHANGE_THRESHOLD
      };
      ReferralTimelineOptions referralTimelineOptions = new()
      {
        MaxDaysToStartProgrammeAfterProviderSelection = TERMINATEAFTERDAYS
      };
      PreparedDischarge.SetOptions(providerOptions, referralTimelineOptions);

      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now;
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;
      string expectedExceptionMessage = "DischargeCompletionDays";
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;
      List<Entities.ProviderSubmission> submissions = [];

      // Act.
      Action act = () => _ = new PreparedDischarge(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      act.Should().Throw<InvalidOperationException>()
        .Which.Message.Should().Contain(expectedExceptionMessage);
    }

    [Fact]
    public void IdDefault_Exception()
    {
      // Arrange.
      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now;
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;
      string expectedExceptionMessage = "*default*id*";
      Guid id = default;
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;
      List<Entities.ProviderSubmission> submissions = [];

      // Act.
      Action act = () => _ = new PreparedDischarge(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      act.Should().Throw<ArgumentException>()
        .Which.Message.Should().Match(expectedExceptionMessage);
    }

    [Fact]
    public void LastEngagementNotPastDischargeCompletionDays_DidNotComplete()
    {
      // Arrange.
      DateTimeOffset? dateCompletedProgramme = null;
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now.AddDays(-1);
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now
        .AddDays(-DISCHARGE_COMPLETION_DAYS);
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now.AddDays(-DISCHARGE_COMPLETION_DAYS);
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;

      Entities.ProviderSubmission firstWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: 100);
      Entities.ProviderSubmission lastWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateStartedProgramme.AddDays(10),
          weight: 95);
      Entities.ProviderSubmission lastNoWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateOfLastEngagement,
          measure: 10,
          weight: 0);
      List<Entities.ProviderSubmission> submissions =
        [firstWeightSubmission, lastWeightSubmission, lastNoWeightSubmission];

      decimal expectedWeightChange = lastWeightSubmission.Weight - firstWeightSubmission.Weight;

      // Act.
      PreparedDischarge preparedDischarge = new(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      preparedDischarge.DateOfLastEngagement.Should().Be(dateOfLastEngagement);
      preparedDischarge.DateStartedProgramme.Should().Be(dateStartedProgramme);
      preparedDischarge.FirstRecordedWeight.Should().Be(firstWeightSubmission.Weight);
      preparedDischarge.FirstRecordedWeightDate.Should().Be(firstWeightSubmission.Date);
      preparedDischarge.FirstWeightSubmission.Should().NotBeNull();
      preparedDischarge.Id.Should().Be(id);
      preparedDischarge.IsAwaitingDischarge.Should().BeTrue();
      preparedDischarge.IsDischargeOnHold.Should().BeFalse();
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeFalse();
      preparedDischarge.IsUnableToDischarge.Should().BeFalse();
      preparedDischarge.LastRecordedWeight.Should().Be(lastWeightSubmission.Weight);
      preparedDischarge.LastRecordedWeightDate.Should().Be(lastWeightSubmission.Date);
      preparedDischarge.LastWeightSubmission.Should().NotBeNull();
      preparedDischarge.NhsNumber.Should().Be(nhsNumber);
      preparedDischarge.ProgrammeOutcome.Should().Be(ProgrammeOutcome.DidNotComplete.ToString());
      preparedDischarge.ReferringGpPracticeNumber.Should().Be(referringGpPracticeNumber);
      preparedDischarge.Status.Should().Be(ReferralStatus.AwaitingDischarge.ToString());
      preparedDischarge.StatusReason.Should().BeNull();
      preparedDischarge.WeightChange.Should().Be(expectedWeightChange);
    }

    [Fact]
    public void LastEngagementPastDischargeCompletionDays_Complete()
    {
      // Arrange.
      DateTimeOffset? dateCompletedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now
        .AddDays(-DISCHARGE_COMPLETION_DAYS);
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now.AddDays(-DISCHARGE_COMPLETION_DAYS);
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;

      Entities.ProviderSubmission firstWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: 100);
      Entities.ProviderSubmission lastWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateStartedProgramme.AddDays(10),
          weight: 105);
      Entities.ProviderSubmission lastNoWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateOfLastEngagement,
          measure: 10,
          weight: 0);
      List<Entities.ProviderSubmission> submissions =
        [firstWeightSubmission, lastWeightSubmission, lastNoWeightSubmission];

      decimal expectedWeightChange = lastWeightSubmission.Weight - firstWeightSubmission.Weight;

      // Act.
      PreparedDischarge preparedDischarge = new(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      preparedDischarge.DateOfLastEngagement.Should().Be(dateOfLastEngagement);
      preparedDischarge.DateStartedProgramme.Should().Be(dateStartedProgramme);
      preparedDischarge.FirstRecordedWeight.Should().Be(firstWeightSubmission.Weight);
      preparedDischarge.FirstRecordedWeightDate.Should().Be(firstWeightSubmission.Date);
      preparedDischarge.FirstWeightSubmission.Should().NotBeNull();
      preparedDischarge.Id.Should().Be(id);
      preparedDischarge.IsAwaitingDischarge.Should().BeTrue();
      preparedDischarge.IsDischargeOnHold.Should().BeFalse();
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeTrue();
      preparedDischarge.IsUnableToDischarge.Should().BeFalse();
      preparedDischarge.LastRecordedWeight.Should().Be(lastWeightSubmission.Weight);
      preparedDischarge.LastRecordedWeightDate.Should().Be(lastWeightSubmission.Date);
      preparedDischarge.LastWeightSubmission.Should().NotBeNull();
      preparedDischarge.NhsNumber.Should().Be(nhsNumber);
      preparedDischarge.ProgrammeOutcome.Should().Be(ProgrammeOutcome.Complete.ToString());
      preparedDischarge.ReferringGpPracticeNumber.Should().Be(referringGpPracticeNumber);
      preparedDischarge.Status.Should().Be(ReferralStatus.AwaitingDischarge.ToString());
      preparedDischarge.StatusReason.Should().BeNull();
      preparedDischarge.WeightChange.Should().Be(expectedWeightChange);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("1234567890")]
    public void InvalidNhsNumber_UnableToDischarge(string nhsNumber)
    {
      // Arrange.
      DateTimeOffset? dateCompletedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now
        .AddDays(-DISCHARGE_COMPLETION_DAYS);
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now.AddDays(-DISCHARGE_COMPLETION_DAYS);
      string expectedStatusReason = "NhsNumber is invalid.";
      Guid id = Guid.NewGuid();
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;

      Entities.ProviderSubmission firstWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: 100);
      Entities.ProviderSubmission lastWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateStartedProgramme.AddDays(10),
          weight: 105);
      Entities.ProviderSubmission lastNoWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateOfLastEngagement,
          measure: 10,
          weight: 0);
      List<Entities.ProviderSubmission> submissions =
        [firstWeightSubmission, lastWeightSubmission, lastNoWeightSubmission];

      decimal expectedWeightChange = lastWeightSubmission.Weight - firstWeightSubmission.Weight;

      // Act.
      PreparedDischarge preparedDischarge = new(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      preparedDischarge.DateOfLastEngagement.Should().Be(dateOfLastEngagement);
      preparedDischarge.DateStartedProgramme.Should().Be(dateStartedProgramme);
      preparedDischarge.FirstRecordedWeight.Should().Be(firstWeightSubmission.Weight);
      preparedDischarge.FirstRecordedWeightDate.Should().Be(firstWeightSubmission.Date);
      preparedDischarge.FirstWeightSubmission.Should().NotBeNull();
      preparedDischarge.Id.Should().Be(id);
      preparedDischarge.IsAwaitingDischarge.Should().BeFalse();
      preparedDischarge.IsDischargeOnHold.Should().BeFalse();
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeTrue();
      preparedDischarge.IsUnableToDischarge.Should().BeTrue();
      preparedDischarge.LastRecordedWeight.Should().Be(lastWeightSubmission.Weight);
      preparedDischarge.LastRecordedWeightDate.Should().Be(lastWeightSubmission.Date);
      preparedDischarge.LastWeightSubmission.Should().NotBeNull();
      preparedDischarge.NhsNumber.Should().Be(nhsNumber);
      preparedDischarge.ProgrammeOutcome.Should().Be(ProgrammeOutcome.Complete.ToString());
      preparedDischarge.ReferringGpPracticeNumber.Should().Be(referringGpPracticeNumber);
      preparedDischarge.Status.Should().Be(ReferralStatus.UnableToDischarge.ToString());
      preparedDischarge.StatusReason.Should().Be(expectedStatusReason);
      preparedDischarge.WeightChange.Should().Be(expectedWeightChange);
    }

    [Fact]
    public void InvalidNhsNumberAndReferringGpPracticeNumber_UnableToDischarge()
    {
      // Arrange.
      DateTimeOffset? dateCompletedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now
        .AddDays(-DISCHARGE_COMPLETION_DAYS);
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now.AddDays(-DISCHARGE_COMPLETION_DAYS);
      string expectedStatusReason = "NhsNumber is invalid. ReferringGpPracticeNumber is invalid.";
      Guid id = Guid.NewGuid();
      string nhsNumber = "012345678";
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_NOTAPPLICABLE;

      Entities.ProviderSubmission firstWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: 100);
      Entities.ProviderSubmission lastWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateStartedProgramme.AddDays(10),
          weight: 105);
      Entities.ProviderSubmission lastNoWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateOfLastEngagement,
          measure: 10,
          weight: 0);
      List<Entities.ProviderSubmission> submissions =
        [firstWeightSubmission, lastWeightSubmission, lastNoWeightSubmission];

      decimal expectedWeightChange = lastWeightSubmission.Weight - firstWeightSubmission.Weight;

      // Act.
      PreparedDischarge preparedDischarge = new(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      preparedDischarge.DateOfLastEngagement.Should().Be(dateOfLastEngagement);
      preparedDischarge.DateStartedProgramme.Should().Be(dateStartedProgramme);
      preparedDischarge.FirstRecordedWeight.Should().Be(firstWeightSubmission.Weight);
      preparedDischarge.FirstRecordedWeightDate.Should().Be(firstWeightSubmission.Date);
      preparedDischarge.FirstWeightSubmission.Should().NotBeNull();
      preparedDischarge.Id.Should().Be(id);
      preparedDischarge.IsAwaitingDischarge.Should().BeFalse();
      preparedDischarge.IsDischargeOnHold.Should().BeFalse();
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeTrue();
      preparedDischarge.IsUnableToDischarge.Should().BeTrue();
      preparedDischarge.LastRecordedWeight.Should().Be(lastWeightSubmission.Weight);
      preparedDischarge.LastRecordedWeightDate.Should().Be(lastWeightSubmission.Date);
      preparedDischarge.LastWeightSubmission.Should().NotBeNull();
      preparedDischarge.NhsNumber.Should().Be(nhsNumber);
      preparedDischarge.ProgrammeOutcome.Should().Be(ProgrammeOutcome.Complete.ToString());
      preparedDischarge.ReferringGpPracticeNumber.Should().Be(referringGpPracticeNumber);
      preparedDischarge.Status.Should().Be(ReferralStatus.UnableToDischarge.ToString());
      preparedDischarge.StatusReason.Should().Be(expectedStatusReason);
      preparedDischarge.WeightChange.Should().Be(expectedWeightChange);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(REFERRINGGPPRACTICENUMBER_NOTAPPLICABLE)]
    [InlineData(REFERRINGGPPRACTICENUMBER_NOTKNOWN)]
    [InlineData(REFERRINGGPPRACTICENUMBER_NOTREGISTERED)]
    public void InvalidReferringGpPracticeNumber_UnableToDischarge(string referringGpPracticeNumber)
    {
      // Arrange.
      DateTimeOffset? dateCompletedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now
        .AddDays(-DISCHARGE_COMPLETION_DAYS);
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now.AddDays(-DISCHARGE_COMPLETION_DAYS);
      string expectedStatusReason = "ReferringGpPracticeNumber is invalid.";
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;

      Entities.ProviderSubmission firstWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: 100);
      Entities.ProviderSubmission lastWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateStartedProgramme.AddDays(10),
          weight: 105);
      Entities.ProviderSubmission lastNoWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateOfLastEngagement,
          measure: 10,
          weight: 0);
      List<Entities.ProviderSubmission> submissions =
        [firstWeightSubmission, lastWeightSubmission, lastNoWeightSubmission];

      decimal expectedWeightChange = lastWeightSubmission.Weight - firstWeightSubmission.Weight;

      // Act.
      PreparedDischarge preparedDischarge = new(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      preparedDischarge.DateOfLastEngagement.Should().Be(dateOfLastEngagement);
      preparedDischarge.DateStartedProgramme.Should().Be(dateStartedProgramme);
      preparedDischarge.FirstRecordedWeight.Should().Be(firstWeightSubmission.Weight);
      preparedDischarge.FirstRecordedWeightDate.Should().Be(firstWeightSubmission.Date);
      preparedDischarge.FirstWeightSubmission.Should().NotBeNull();
      preparedDischarge.Id.Should().Be(id);
      preparedDischarge.IsAwaitingDischarge.Should().BeFalse();
      preparedDischarge.IsDischargeOnHold.Should().BeFalse();
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeTrue();
      preparedDischarge.IsUnableToDischarge.Should().BeTrue();
      preparedDischarge.LastRecordedWeight.Should().Be(lastWeightSubmission.Weight);
      preparedDischarge.LastRecordedWeightDate.Should().Be(lastWeightSubmission.Date);
      preparedDischarge.LastWeightSubmission.Should().NotBeNull();
      preparedDischarge.NhsNumber.Should().Be(nhsNumber);
      preparedDischarge.ProgrammeOutcome.Should().Be(ProgrammeOutcome.Complete.ToString());
      preparedDischarge.ReferringGpPracticeNumber.Should().Be(referringGpPracticeNumber);
      preparedDischarge.Status.Should().Be(ReferralStatus.UnableToDischarge.ToString());
      preparedDischarge.StatusReason.Should().Be(expectedStatusReason);
      preparedDischarge.WeightChange.Should().Be(expectedWeightChange);
    }

    [Fact]
    public void NoSubmissions_DateCompletedProgrammeNotNull_DidNotCommence()
    {
      // Arrange.
      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now
        .AddDays(-DISCHARGE_AFTER_DAYS - 1);
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now.AddDays(-DISCHARGE_AFTER_DAYS - 1);
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;
      List<Entities.ProviderSubmission> submissions = [];

      // Act.
      PreparedDischarge preparedDischarge = new(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      preparedDischarge.DateOfLastEngagement.Should().Be(dateCompletedProgramme);
      preparedDischarge.DateStartedProgramme.Should().Be(dateStartedProgramme);
      preparedDischarge.FirstRecordedWeight.Should().BeNull();
      preparedDischarge.FirstRecordedWeightDate.Should().BeNull();
      preparedDischarge.FirstWeightSubmission.Should().BeNull();
      preparedDischarge.Id.Should().Be(id);
      preparedDischarge.IsAwaitingDischarge.Should().BeTrue();
      preparedDischarge.IsDischargeOnHold.Should().BeFalse();
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeFalse();
      preparedDischarge.IsUnableToDischarge.Should().BeFalse();
      preparedDischarge.LastRecordedWeight.Should().BeNull();
      preparedDischarge.LastRecordedWeightDate.Should().BeNull();
      preparedDischarge.LastWeightSubmission.Should().BeNull();
      preparedDischarge.NhsNumber.Should().Be(nhsNumber);
      preparedDischarge.ProgrammeOutcome.Should().Be(ProgrammeOutcome.DidNotCommence.ToString());
      preparedDischarge.ReferringGpPracticeNumber.Should().Be(referringGpPracticeNumber);
      preparedDischarge.Status.Should().Be(ReferralStatus.AwaitingDischarge.ToString());
      preparedDischarge.StatusReason.Should().BeNull();
      preparedDischarge.WeightChange.Should().Be(0);
    }

    [Fact]
    public void NoSubmissions_DateCompletedProgrammeNull_DidNotCommence()
    {
      // Arrange.
      DateTimeOffset? dateCompletedProgramme = null;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now
        .AddDays(-DISCHARGE_AFTER_DAYS - 1);
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now.AddDays(-DISCHARGE_AFTER_DAYS - 1);
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;
      List<Entities.ProviderSubmission> submissions = [];

      // Act.
      PreparedDischarge preparedDischarge = new(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      preparedDischarge.DateOfLastEngagement.Should()
        .Be(dateStartedProgramme.AddDays(DISCHARGE_AFTER_DAYS));
      preparedDischarge.DateStartedProgramme.Should().Be(dateStartedProgramme);
      preparedDischarge.FirstRecordedWeight.Should().BeNull();
      preparedDischarge.FirstRecordedWeightDate.Should().BeNull();
      preparedDischarge.FirstWeightSubmission.Should().BeNull();
      preparedDischarge.Id.Should().Be(id);
      preparedDischarge.IsAwaitingDischarge.Should().BeTrue();
      preparedDischarge.IsDischargeOnHold.Should().BeFalse();      
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeFalse();
      preparedDischarge.IsUnableToDischarge.Should().BeFalse();
      preparedDischarge.LastRecordedWeight.Should().BeNull();
      preparedDischarge.LastRecordedWeightDate.Should().BeNull();
      preparedDischarge.LastWeightSubmission.Should().BeNull();
      preparedDischarge.NhsNumber.Should().Be(nhsNumber);
      preparedDischarge.ProgrammeOutcome.Should().Be(ProgrammeOutcome.DidNotCommence.ToString());
      preparedDischarge.ReferringGpPracticeNumber.Should().Be(referringGpPracticeNumber);
      preparedDischarge.Status.Should().Be(ReferralStatus.AwaitingDischarge.ToString());
      preparedDischarge.StatusReason.Should().BeNull();
      preparedDischarge.WeightChange.Should().Be(0);
    }

    [Fact]
    public void WeightChangeThresholdNotSet_Exception()
    {
      // Arrange.
      ProviderOptions providerOptions = new()
      {
        DischargeAfterDays = DISCHARGE_AFTER_DAYS,
        DischargeCompletionDays = DISCHARGE_COMPLETION_DAYS,
        WeightChangeThreshold = PreparedDischarge.NOT_SET
      };
      ReferralTimelineOptions referralTimelineOptions = new()
      {
        MaxDaysToStartProgrammeAfterProviderSelection = TERMINATEAFTERDAYS
      };
      PreparedDischarge.SetOptions(providerOptions, referralTimelineOptions);

      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now;
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;
      string expectedExceptionMessage = "WeightChangeThreshold";
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;
      List<Entities.ProviderSubmission> submissions = [];

      // Act.
      Action act = () => _ = new PreparedDischarge(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      act.Should().Throw<InvalidOperationException>()
        .Which.Message.Should().Contain(expectedExceptionMessage);
    }

    [Theory]
    [InlineData(100, 100 - WEIGHT_CHANGE_THRESHOLD - 1, "loss")]
    [InlineData(100, 100 + WEIGHT_CHANGE_THRESHOLD + 1, "gain")]
    public void WeightChangeOverThreshold_StatusReasonSet_Complete(
      decimal firstWeight,
      decimal lastWeight,
      string gainOrLoss)
    {
      // Arrange.
      DateTimeOffset? dateCompletedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;
      DateTimeOffset dateOfProviderSelection = DateTimeOffset.Now.AddDays(-DISCHARGE_COMPLETION_DAYS);
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now.AddDays(-DISCHARGE_COMPLETION_DAYS);
      Guid id = Guid.NewGuid();
      string nhsNumber = NHSNUMBER_VALID;
      string referringGpPracticeNumber = REFERRINGGPPRACTICENUMBER_VALID;

      Entities.ProviderSubmission firstWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: firstWeight);
      Entities.ProviderSubmission lastWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateStartedProgramme.AddDays(10),
          weight: lastWeight);
      Entities.ProviderSubmission lastNoWeightSubmission = RandomEntityCreator
        .CreateProviderSubmission(
          date: dateOfLastEngagement,
          measure: 10,
          weight: 0);
      List<Entities.ProviderSubmission> submissions =
        [firstWeightSubmission, lastWeightSubmission, lastNoWeightSubmission];

      decimal expectedWeightChange = lastWeight - firstWeight;

      decimal weightChange = Math.Abs(lastWeight - firstWeight);
      string expectedStatusReason = $"*{gainOrLoss}*{weightChange}*{WEIGHT_CHANGE_THRESHOLD}*";

      // Act.
      PreparedDischarge preparedDischarge = new(
        id,
        dateOfProviderSelection,
        dateStartedProgramme,
        dateCompletedProgramme,
        nhsNumber,
        submissions,
        referringGpPracticeNumber);

      // Assert.
      preparedDischarge.DateOfLastEngagement.Should().Be(dateOfLastEngagement);
      preparedDischarge.DateStartedProgramme.Should().Be(dateStartedProgramme);
      preparedDischarge.FirstRecordedWeight.Should().Be(firstWeightSubmission.Weight);
      preparedDischarge.FirstRecordedWeightDate.Should().Be(firstWeightSubmission.Date);
      preparedDischarge.FirstWeightSubmission.Should().NotBeNull();
      preparedDischarge.Id.Should().Be(id);
      preparedDischarge.IsAwaitingDischarge.Should().BeTrue();
      preparedDischarge.IsDischargeOnHold.Should().BeFalse();
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeTrue();
      preparedDischarge.IsUnableToDischarge.Should().BeFalse();
      preparedDischarge.LastRecordedWeight.Should().BeNull();
      preparedDischarge.LastRecordedWeightDate.Should().BeNull();
      preparedDischarge.LastWeightSubmission.Should().NotBeNull();
      preparedDischarge.NhsNumber.Should().Be(nhsNumber);
      preparedDischarge.ProgrammeOutcome.Should().Be(ProgrammeOutcome.Complete.ToString());
      preparedDischarge.ReferringGpPracticeNumber.Should().Be(referringGpPracticeNumber);
      preparedDischarge.Status.Should().Be(ReferralStatus.AwaitingDischarge.ToString());
      preparedDischarge.StatusReason.Should().Match(expectedStatusReason);
      preparedDischarge.WeightChange.Should().Be(expectedWeightChange);
    }
  }
}
