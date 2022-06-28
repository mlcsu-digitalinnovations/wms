using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;
using WmsHub.Common.Extensions;
using Xunit;

namespace WmsHub.Common.Tests.Attributes
{
  public class ReferralStatusTraceAttributeTests:BaseTests
  {
    private readonly string[] _fields = new string[]
    {
      "NumberDaysLookup",
      "CanTrace",
      "NumberOfDays",
      "TypeId"
    };

    public ReferralStatusTraceAttributeTests()
    {
      Environment.SetEnvironmentVariable(
        "WmsHub_BusinessIntelligence_Api_MinDaysBetweenTraces", "1");
      Environment.SetEnvironmentVariable(
        "WmsHub_BusinessIntelligence_Api_DaysBetweenTraces", "7");
      Environment.SetEnvironmentVariable(
        "WmsHub_BusinessIntelligence_Api_MaxDaysBetweenTraces", "30");
    }

    [Fact]
    public void CorrectNumberFields()
    {
      //Arrange
      //Act
      PropertyInfo[] propinfo = GetAllProperties(
        new ReferralStatusTraceAttribute(false));
      //Assert
      propinfo.Length.Should().Be(_fields.Length,
        $"Fields Found {string.Join(",",propinfo.Select(t=>t.Name))}");
      foreach (PropertyInfo info in propinfo)
      {
        Array.IndexOf(_fields, info.Name).Should()
          .BeGreaterThan(-1, info.Name);
      }
     
    }

    [Theory]
    [InlineData(ReferralStatus.New, true)]
    [InlineData(ReferralStatus.TextMessage1, true)]
    [InlineData(ReferralStatus.TextMessage2, true)]
    [InlineData(ReferralStatus.ChatBotCall1, true)]
    [InlineData(ReferralStatus.ChatBotCall2, true)]
    [InlineData(ReferralStatus.ChatBotTransfer, true)]
    [InlineData(ReferralStatus.RmcCall, true)]
    [InlineData(ReferralStatus.RmcDelayed, true)]
    [InlineData(ReferralStatus.Letter, true)]
    [InlineData(ReferralStatus.ProviderAwaitingTrace, true)]
    public void Valid_Daily_Trace(ReferralStatus status, bool exptectedCanTrace)
    {
      //Arrange
      int expectedDays = 1;
      DateTimeOffset lastTraceDate = DateTimeOffset.Now.AddDays(-1);
      //Act
      bool canTrace = status.GetCanTrace();
      int? traceDays = status.GetTraceDays();
      //Assert
      canTrace.Should().Be(exptectedCanTrace, status.ToString());
      traceDays.Should().Be(expectedDays);
    }

    [Theory]
    [InlineData(ReferralStatus.Exception, false)]
    [InlineData(ReferralStatus.CancelledByEreferrals, false)]
    [InlineData(ReferralStatus.RejectedToEreferrals, false)]
    [InlineData(ReferralStatus.FailedToContact, false)]
    [InlineData(ReferralStatus.FailedToContactTextMessage, false)]
    public void Valid_No_Trace(ReferralStatus status, bool exptectedCanTrace)
    {
      //Arrange
      int expectedDays = -1;
      DateTimeOffset lastTraceDate = DateTimeOffset.Now.AddDays(-1);
      //Act
      bool canTrace = status.GetCanTrace();
      int? traceDays = status.GetTraceDays();
      //Assert
      canTrace.Should().Be(exptectedCanTrace);
      traceDays.Should().Be(expectedDays);
    }

    [Theory]
    [InlineData(ReferralStatus.ProviderAwaitingStart, true)]
    [InlineData(ReferralStatus.ProviderAccepted, true)]
    [InlineData(ReferralStatus.ProviderDeclinedByServiceUser, true)]
    [InlineData(ReferralStatus.ProviderRejected, true)]
    [InlineData(ReferralStatus.ProviderRejectedTextMessage, true)]
    [InlineData(ReferralStatus.ProviderContactedServiceUser, true)]
    [InlineData(ReferralStatus.ProviderStarted, true)]
    [InlineData(ReferralStatus.ProviderCompleted, true)]
    [InlineData(ReferralStatus.ProviderTerminated, true)]
    [InlineData(ReferralStatus.ProviderTerminatedTextMessage, true)]
    [InlineData(ReferralStatus.LetterSent, true)]
    public void Valid_weekly_Trace(ReferralStatus status, bool exptectedCanTrace)
    {
      //Arrange
      int expectedDays = 7;
      DateTimeOffset lastTraceDate = DateTimeOffset.Now.AddDays(-1);
      //Act
      bool canTrace = status.GetCanTrace();
      int? traceDays = status.GetTraceDays();
      //Assert
      canTrace.Should().Be(exptectedCanTrace);
      traceDays.Should().Be(expectedDays);
    }

    [Theory]
    [InlineData(ReferralStatus.Complete, true)]
    public void Valid_monthly_trace(ReferralStatus status, bool exptectedCanTrace)
    {
      //Arrange
      int expectedDays = 30;
      DateTimeOffset lastTraceDate = DateTimeOffset.Now.AddDays(-1);
      //Act
      bool canTrace = status.GetCanTrace();
      int? traceDays = status.GetTraceDays();
      //Assert
      canTrace.Should().Be(exptectedCanTrace);
      traceDays.Should().Be(expectedDays);
    }

    [Theory]
    [InlineData(TestReferralStatusAttributes.New, true)]
    public void InValid_trace_Return_1_day(TestReferralStatusAttributes status, 
      bool exptectedCanTrace)
    {
      //Arrange
      int expectedDays = 1;
      DateTimeOffset lastTraceDate = DateTimeOffset.Now.AddDays(-1);
      //Act
      bool canTrace = status.GetCanTrace();
      int? traceDays = status.GetTraceDays();
      //Assert
      canTrace.Should().Be(exptectedCanTrace);
      traceDays.Should().Be(expectedDays);
    }
  }

  public enum TestReferralStatusAttributes
  {
    [ReferralStatusTrace(true)]
    New,
  }
}