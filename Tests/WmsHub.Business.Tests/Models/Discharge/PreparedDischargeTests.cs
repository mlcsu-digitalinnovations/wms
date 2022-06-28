using FluentAssertions;
using System;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Discharge;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models.Discharge
{
  public class PreparedDischargeTests : AModelsBaseTests
  {
    const int DISCHARGE_AFTER_DAYS = 94;
    const int DISCHARGE_COMPLETION_DAYS = 49;
    const int WEIGHT_CHANGE_THRESHOLD = 25;

    public PreparedDischargeTests()
    {
      PreparedDischarge.DischargeAfterDays = DISCHARGE_AFTER_DAYS;
      PreparedDischarge.DischargeCompletionDays = DISCHARGE_COMPLETION_DAYS;
      PreparedDischarge.WeightChangeThreshold = WEIGHT_CHANGE_THRESHOLD;
    }

    [Fact]
    public void DischargeAfterDaysNotSet_Exception()
    {
      // arrange
      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission();
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission();
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;
      string referralSource = ReferralSource.GpReferral.ToString();
      string expectedExceptionMessage =
        $"{nameof(PreparedDischarge.DischargeAfterDays)} " +
        "has not been set.";

      // act
      PreparedDischarge.DischargeAfterDays = PreparedDischarge.NOT_SET;
      var ex = Record.Exception(() => new PreparedDischarge(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource));

      // assert
      ex.Should().BeOfType<InvalidOperationException>();
      ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void DischargeCompletionDaysNotSet_Exception()
    {
      // arrange
      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission();
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission();
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;
      string referralSource = ReferralSource.GpReferral.ToString();
      string expectedExceptionMessage =
        $"{nameof(PreparedDischarge.DischargeCompletionDays)} " +
        "has not been set.";

      // act
      PreparedDischarge.DischargeCompletionDays = PreparedDischarge.NOT_SET;
      var ex = Record.Exception(() => new PreparedDischarge(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource));

      // assert
      ex.Should().BeOfType<InvalidOperationException>();
      ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void IdDefaultGuid_Exception()
    {
      // arrange
      Guid id = default;
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(1));
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission();
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;
      string referralSource = ReferralSource.GpReferral.ToString();
      string expectedExceptionMessage =
        "Cannot be a default GUID (Parameter 'id')";

      // act
      var ex = Record.Exception(() => new PreparedDischarge(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource));

      // assert
      ex.Should().BeOfType<ArgumentException>();
      ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void WeightChangeThresholdNotSet_Exception()
    {
      // arrange
      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission();
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission();
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;
      string referralSource = ReferralSource.GpReferral.ToString();
      string expectedExceptionMessage =
        $"{nameof(PreparedDischarge.WeightChangeThreshold)} " +
        "has not been set.";

      // act
      PreparedDischarge.WeightChangeThreshold = PreparedDischarge.NOT_SET;
      var ex = Record.Exception(() => new PreparedDischarge(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource));

      // assert
      ex.Should().BeOfType<InvalidOperationException>();
      ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void DateStartedProgrammeDefaultDateTimeOffset_Exception()
    {
      // arrange
      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = default;
      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(1));
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission();
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;
      string referralSource = ReferralSource.GpReferral.ToString();
      string expectedExceptionMessage =
        "Cannot be a default DateTimeOffset (Parameter 'dateStartedProgramme')";

      // act
      var ex = Record.Exception(() => new PreparedDischarge(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource));

      // assert
      ex.Should().BeOfType<ArgumentException>();
      ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void DateCompletedProgrammeDefaultDateTimeOffset_Exception()
    {
      // arrange
      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;      
      DateTimeOffset dateCompletedProgramme = default;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(1));
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission();
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;
      string referralSource = ReferralSource.GpReferral.ToString();
      string expectedExceptionMessage = "Cannot be a default DateTimeOffset " +
        "(Parameter 'dateCompletedProgramme')";

      // act
      var ex = Record.Exception(() => new PreparedDischarge(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource));

      // assert
      ex.Should().BeOfType<ArgumentException>();
      ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void ReferralSourceInvalid_Exception()
    {
      // arrange
      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;
      DateTimeOffset dateCompletedProgramme = DateTimeOffset.Now;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission();
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission();
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;
      string referralSource = "InvalidReferralSource";
      string expectedExceptionMessage =
        "Is not a valid referral source (Parameter 'referralSource')";

      // act
      var ex = Record.Exception(() => new PreparedDischarge(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource));

      // assert
      ex.Should().BeOfType<ArgumentException>();
      ex.Message.Should().Be(expectedExceptionMessage);
    }

    [Fact]
    public void LastEngagementDefault_DateCompletedProgrammeNull_DidNotCommence()
    {
      // arrange
      DateTimeOffset dateOfLastEngagement = default;      

      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now
        .AddDays(-DISCHARGE_AFTER_DAYS - 1);
      DateTimeOffset? dateCompletedProgramme = null;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: 100);
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(10),
          weight: 95);
      string referralSource = ReferralSource.GpReferral.ToString();

      DateTimeOffset expectedDateOfLastEngagement = dateStartedProgramme
        .AddDays(DISCHARGE_AFTER_DAYS);

      // act
      PreparedDischarge preparedDischarge = new(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource.ToString());

      // assert
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeFalse();
      preparedDischarge.IsAwaitingDischarge.Should().BeTrue();
      preparedDischarge.ProgrammeOutcome.Should().Be(
        ProgrammeOutcome.DidNotCommence.ToString());
      preparedDischarge.Status.Should()
        .Be(ReferralStatus.AwaitingDischarge.ToString());

      UniversalAsserts(
        preparedDischarge,
        id,
        dateStartedProgramme,
        firstWeightSubmission,
        lastWeightSubmission,
        expectedDateOfLastEngagement,
        referralSource);
    }

    [Fact]
    public void LastEngagementDefault_DateCompletedProgrammeNotNull_DidNotCommence()
    {
      // arrange
      DateTimeOffset dateOfLastEngagement = default;
      DateTimeOffset? dateCompletedProgramme = DateTimeOffset.Now;

      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now
        .AddDays(-DISCHARGE_COMPLETION_DAYS - 1);      
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: 100);
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(10),
          weight: 105);
      string referralSource = ReferralSource.GpReferral.ToString();

      DateTimeOffset expectedDateOfLastEngagement = dateCompletedProgramme.Value;

      // act
      PreparedDischarge preparedDischarge = new(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource.ToString());

      // assert
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeFalse();
      preparedDischarge.IsAwaitingDischarge.Should().BeTrue();
      preparedDischarge.ProgrammeOutcome.Should().Be(
        ProgrammeOutcome.DidNotCommence.ToString());
      preparedDischarge.Status.Should()
        .Be(ReferralStatus.AwaitingDischarge.ToString());

      preparedDischarge.DateOfLastEngagement.Should().Be(
        dateCompletedProgramme.Value,
        because: "dateCompletedProgramme is not null.");
      
      UniversalAsserts(
        preparedDischarge,
        id,
        dateStartedProgramme,
        firstWeightSubmission,
        lastWeightSubmission,
        expectedDateOfLastEngagement,
        referralSource);
    }

    [Fact]
    public void FirstSubAndLastSubNull_RecordedWeightsAndDatesNull()
    {
      // arrange      
      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now;
      DateTimeOffset? dateCompletedProgramme = null;
      ProviderSubmission firstWeightSubmission = null;
      ProviderSubmission lastWeightSubmission = null;
      DateTimeOffset dateOfLastEngagement = default;
      string referralSource = ReferralSource.GpReferral.ToString();

      // act
      PreparedDischarge preparedDischarge = new(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource);

      // assert
      preparedDischarge.FirstRecordedWeight.Should().BeNull();
      preparedDischarge.FirstRecordedWeightDate.Should().BeNull();
      preparedDischarge.LastRecordedWeight.Should().BeNull();
      preparedDischarge.LastRecordedWeightDate.Should().BeNull();
      preparedDischarge.WeightChange.Should().Be(0);
    }

    [Fact]
    public void Gp_LastEngageNotPastDischargeCompDays_DidNotCompleteAwaitingDischarge()
    {
      // arrange
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now.AddDays(-1);

      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now
        .AddDays(-DISCHARGE_COMPLETION_DAYS);
      DateTimeOffset? dateCompletedProgramme = null;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: 100);
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(10),
          weight: 95);
      string referralSource = ReferralSource.GpReferral.ToString();

      // act
      PreparedDischarge preparedDischarge = new(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource);

      // assert
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeFalse();
      preparedDischarge.IsAwaitingDischarge.Should().BeTrue();
      preparedDischarge.ProgrammeOutcome.Should().Be(
        ProgrammeOutcome.DidNotComplete.ToString());
      preparedDischarge.Status.Should().Be(
        ReferralStatus.AwaitingDischarge.ToString());

      UniversalAsserts(
        preparedDischarge,
        id,
        dateStartedProgramme,
        firstWeightSubmission,
        lastWeightSubmission,
        dateOfLastEngagement,
        referralSource);
    }

    [Fact]
    public void Gp_LastEngagePastDischargeCompDays_CompleteAwaitingDischarge()
    {
      // arrange
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;

      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now
        .AddDays(-DISCHARGE_COMPLETION_DAYS);
      DateTimeOffset? dateCompletedProgramme = DateTimeOffset.Now;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: 100);
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(10), 
          weight: 105);
      string referralSource = ReferralSource.GpReferral.ToString();

      // act
      PreparedDischarge preparedDischarge = new(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource);

      // assert
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeTrue();
      preparedDischarge.IsAwaitingDischarge.Should().BeTrue();
      preparedDischarge.ProgrammeOutcome.Should().Be(
        ProgrammeOutcome.Complete.ToString());
      preparedDischarge.Status.Should().Be(
        ReferralStatus.AwaitingDischarge.ToString());

      UniversalAsserts(
        preparedDischarge,
        id,
        dateStartedProgramme,
        firstWeightSubmission,
        lastWeightSubmission,
        dateOfLastEngagement,
        referralSource);
    }

    [Theory]
    [InlineData(100, 100 - WEIGHT_CHANGE_THRESHOLD - 1, "loss")]
    [InlineData(100, 100 + WEIGHT_CHANGE_THRESHOLD + 1, "gain")]
    public void Gp_WeightChangeOverThreshold_DischargeOnHold(
      decimal firstWeight,
      decimal lastWeight,
      string gainOrLoss)
    {
      // arrange
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;

      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now
        .AddDays(-DISCHARGE_COMPLETION_DAYS);
      DateTimeOffset? dateCompletedProgramme = DateTimeOffset.Now;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: firstWeight);
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(10),
          weight: lastWeight);
      string referralSource = ReferralSource.GpReferral.ToString();
      decimal weightChange = Math.Abs(lastWeight - firstWeight);
      string expectedStatusReason = $"Weight {gainOrLoss} of {weightChange} " +
        $"is more than the expected maximum of {WEIGHT_CHANGE_THRESHOLD}.";

      // act
      PreparedDischarge preparedDischarge = new(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource);

      // assert
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeTrue();
      preparedDischarge.IsAwaitingDischarge.Should().BeFalse();
      preparedDischarge.ProgrammeOutcome.Should().Be(
        ProgrammeOutcome.Complete.ToString());
      preparedDischarge.Status.Should().Be(
        ReferralStatus.DischargeOnHold.ToString());
      preparedDischarge.StatusReason.Should().Be(expectedStatusReason);

      UniversalAsserts(
        preparedDischarge,
        id,
        dateStartedProgramme,
        firstWeightSubmission,
        lastWeightSubmission,
        dateOfLastEngagement,
        referralSource);
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData), ReferralSource.GpReferral)]
    public void NonGp_LastEngageNotPastDischargeCompDays_DidNotCompleteComplete(
      ReferralSource referralSource)
    {
      // arrange
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now.AddDays(-1);

      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now
        .AddDays(-DISCHARGE_COMPLETION_DAYS);
      DateTimeOffset? dateCompletedProgramme = null;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: 100);
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(10),
          weight: 95);

      // act
      PreparedDischarge preparedDischarge = new(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource.ToString());

      // assert
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeFalse();
      preparedDischarge.IsAwaitingDischarge.Should().BeFalse();
      preparedDischarge.ProgrammeOutcome.Should().Be(
        ProgrammeOutcome.DidNotComplete.ToString());
      preparedDischarge.Status.Should().Be(ReferralStatus.Complete.ToString());

      UniversalAsserts(
        preparedDischarge,
        id,
        dateStartedProgramme,
        firstWeightSubmission,
        lastWeightSubmission,
        dateOfLastEngagement,
        referralSource.ToString());
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData), ReferralSource.GpReferral)]
    public void NonGp_LastEngagePastDischargeCompDays_CompleteComplete(
      ReferralSource referralSource)
    {
      // arrange
      DateTimeOffset dateOfLastEngagement = DateTimeOffset.Now;

      Guid id = Guid.NewGuid();
      DateTimeOffset dateStartedProgramme = DateTimeOffset.Now
        .AddDays(-DISCHARGE_COMPLETION_DAYS - 1);
      DateTimeOffset? dateCompletedProgramme = DateTimeOffset.Now;
      ProviderSubmission firstWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(1),
          weight: 100);
      ProviderSubmission lastWeightSubmission = RandomModelCreator
        .CreateRandomProviderSubmission(
          date: dateStartedProgramme.AddDays(10),
          weight: 105);

      // act
      PreparedDischarge preparedDischarge = new(
          id,
          dateStartedProgramme,
          dateCompletedProgramme,
          firstWeightSubmission,
          lastWeightSubmission,
          dateOfLastEngagement,
          referralSource.ToString());

      // assert
      preparedDischarge.IsProgrammeOutcomeComplete.Should().BeTrue();
      preparedDischarge.IsAwaitingDischarge.Should().BeFalse();
      preparedDischarge.ProgrammeOutcome.Should().Be(
        ProgrammeOutcome.Complete.ToString());
      preparedDischarge.Status.Should().Be(ReferralStatus.Complete.ToString());

      UniversalAsserts(
        preparedDischarge,
        id,
        dateStartedProgramme,
        firstWeightSubmission,
        lastWeightSubmission,
        dateOfLastEngagement,
        referralSource.ToString());
    }


    private static void UniversalAsserts(
      PreparedDischarge preparedDischarge,
      Guid id,
      DateTimeOffset dateStartedProgramme,
      ProviderSubmission firstWeightSubmission,
      ProviderSubmission lastWeightSubmission,
      DateTimeOffset dateOfLastEngagement,
      string referralSource)
    {
      preparedDischarge.Id.Should().Be(id);
      preparedDischarge.DateOfLastEngagement.Should().Be(dateOfLastEngagement);      
      preparedDischarge.DateStartedProgramme.Should().Be(dateStartedProgramme);
      preparedDischarge.FirstWeightSubmission.Should().Be(firstWeightSubmission);
      preparedDischarge.LastWeightSubmission.Should().Be(lastWeightSubmission);
      preparedDischarge.ReferralSource.ToString().Should().Be(referralSource);
      preparedDischarge.FirstRecordedWeight.Should()
        .Be(firstWeightSubmission.Weight);
      preparedDischarge.FirstRecordedWeightDate.Should()
        .Be(firstWeightSubmission.Date);
      preparedDischarge.LastRecordedWeight.Should()
        .Be(lastWeightSubmission.Weight);
      preparedDischarge.LastRecordedWeightDate.Should()
        .Be(lastWeightSubmission.Date);
    }
  }
}
