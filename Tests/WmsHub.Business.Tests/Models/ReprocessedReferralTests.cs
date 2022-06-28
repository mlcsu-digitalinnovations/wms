using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Reflection;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.Business.Tests.Models
{
 public class ReprocessedReferralTests: AModelsBaseTests
  {
    [Fact]
    public void CorrectNumberFields()
    {
      //arrange
      FieldNames = new string[]
      {
        "Ubrn",
        "InitialStatus",
        "InitialStatusReason",
        "Reprocessed",
        "SuccessfullyReprocessed",
        "Uncancelled",
        "CurrentlyCancelled",
        "CurrentlyCancelledStatusReason",
        "DateOfReferral",
        "ReferringGpPracticeCode",
        "StatusArray"
      };
      string message = "";
      //act
      PropertyInfo[] propinfo =
        CorrectNumberOfFields<ReprocessedReferral>(out message);
      //Assert
      propinfo.Length.Should().Be(FieldNames.Length, message);
    }

    [Theory]
    [MemberData(nameof(ReprocessedStatuses))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Usage", 
      "xUnit1026:Theory methods should use all of their parameters", 
      Justification = "<Pending>")]
    public void ReprocessedResult(
      string statuses,
      bool expected,
      bool ignore,
      string message)
    {
      //arrange 
      ReprocessedReferral referral = 
        new ReprocessedReferral { StatusArray = statuses.Split(',') };

      //Act
      var result = referral.Reprocessed;

      //asset
      result.Should().Be(expected, message);
    }


    [Theory]
    [MemberData(nameof(ReprocessedStatuses))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", 
      "xUnit1026:Theory methods should use all of their parameters", 
      Justification = "<Pending>")]
    public void SuccessfullyReprocessedResult(
      string statuses,
      bool ignore,
      bool expected,
      string message)
    {
      //arrange 
      ReprocessedReferral referral =
       new ReprocessedReferral { StatusArray = statuses.Split(',') };

      //Act
      var result = referral.SuccessfullyReprocessed;

      //asset
      result.Should().Be(expected, message);
    }

    [Theory]
    [MemberData(nameof(CancelledStatuses))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
      "xUnit1026:Theory methods should use all of their parameters",
      Justification = "<Pending>")]
    public void CurrentlyCancelledResult(string statuses,
      bool expected, bool ignore, string message)
    {
      //arrange 
      ReprocessedReferral referral =
       new ReprocessedReferral { StatusArray = statuses.Split(',') };

      //Act
      var result = referral.CurrentlyCancelled;

      //asset
      result.Should().Be(expected, message);
    }

    [Theory]
    [MemberData(nameof(CancelledStatuses))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
      "xUnit1026:Theory methods should use all of their parameters",
      Justification = "<Pending>")]
    public void UncancelledResult(string statuses,
     bool ignore, bool expected, string message)
    {
      //arrange 
      ReprocessedReferral referral =
       new ReprocessedReferral { StatusArray = statuses.Split(',') };

      //Act
      var result = referral.Uncancelled;

      //asset
      result.Should().Be(expected, message);
    }

    public static IEnumerable<object[]> ReprocessedStatuses()
    {
      List<object[]> validData = new();

      List<string> sets = new List<string>();
      string set1 = $"{ReferralStatus.New}," +
        $"{ReferralStatus.ProviderAwaitingStart}" +
        $"{ReferralStatus.ProviderRejected}," +
        $"{ReferralStatus.Exception}," +
        $"{ReferralStatus.ProviderRejected}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.RmcCall}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderStarted}," +
        $"{ReferralStatus.ProviderCompleted}";


      string set2 = $"{ReferralStatus.New}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderRejected}," +
        $"{ReferralStatus.Exception}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.RmcCall}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderStarted}," +
        $"{ReferralStatus.ProviderCompleted}";


      string set3 = $"{ReferralStatus.New}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.Exception}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.RmcCall}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderStarted}," +
        $"{ReferralStatus.ProviderCompleted}";


      string set4 = $"{ReferralStatus.New}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.RmcCall}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.RmcCall}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderStarted}," +
        $"{ReferralStatus.ProviderTerminated}" +
        $"{ReferralStatus.RejectedToEreferrals}";

      string set5 = $"{ReferralStatus.New}," +
        $"{ReferralStatus.TextMessage1}," +
        $"{ReferralStatus.ProviderAccepted}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderStarted}," +
        $"{ReferralStatus.ProviderCompleted}";

      string set6 = $"{ReferralStatus.New}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderRejected}," +
        $"{ReferralStatus.Exception}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.Exception}";

      string set7 = $"{ReferralStatus.Exception}," +
       $"{ReferralStatus.RejectedToEreferrals}," +
       $"{ReferralStatus.Exception}";


      validData.Add(new object[] { set1, true,true,"Set 1 used" });
      validData.Add(new object[] { set2, true, true, "Set 2 used" });
      validData.Add(new object[] { set3, true, true, "Set 3 used" });
      validData.Add(new object[] { set4, false, false, "Set 4 used" });
      validData.Add(new object[] { set5, false, false, "Set 5 used" });
      validData.Add(new object[] { set6, true, false, "Set 6 used" });
      validData.Add(new object[] { set7, false, false, "Set 7 used" });


      return validData;
    }

    public static IEnumerable<object[]> CancelledStatuses()
    {
      List<object[]> validData = new();

      List<string> sets = new List<string>();
      string set1 = $"{ReferralStatus.New}," +
        $"{ReferralStatus.ProviderAwaitingStart}" +
        $"{ReferralStatus.ProviderRejected}," +
        $"{ReferralStatus.Exception}," +
        $"{ReferralStatus.ProviderRejected}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.RmcCall}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderStarted}," +
        $"{ReferralStatus.CancelledByEreferrals}";


      string set2 = $"{ReferralStatus.New}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderRejected}," +
        $"{ReferralStatus.Exception}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.RmcCall}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderStarted}," +
        $"{ReferralStatus.CancelledByEreferrals}";


      string set3 = $"{ReferralStatus.New}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.Exception}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.RmcCall}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderStarted}," +
        $"{ReferralStatus.CancelledByEreferrals}";


      string set4 = $"{ReferralStatus.New}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.CancelledByEreferrals}," +
        $"{ReferralStatus.RejectedToEreferrals}," +
        $"{ReferralStatus.RmcCall}," +
        $"{ReferralStatus. ProviderAwaitingStart}";

      string set5 = $"{ReferralStatus.New}," +
        $"{ReferralStatus.TextMessage1}," +
        $"{ReferralStatus.ProviderAccepted}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderStarted}," +
        $"{ReferralStatus.CancelledByEreferrals}";

      string set6 = $"{ReferralStatus.New}," +
        $"{ReferralStatus.ProviderAwaitingStart}," +
        $"{ReferralStatus.ProviderRejected}," +
        $"{ReferralStatus.CancelledByEreferrals}," +
        $"{ReferralStatus.New}," +
        $"{ReferralStatus.TextMessage1}";

      string set7 = $"{ReferralStatus.Exception}," +
      $"{ReferralStatus.RejectedToEreferrals}," +
      $"{ReferralStatus.Exception}";

      string set8 = $"{ReferralStatus.New}," +
       $"{ReferralStatus.RejectedToEreferrals}," +
       $"{ReferralStatus.RmcCall}," +
       $"{ReferralStatus.RejectedToEreferrals}," +
       $"{ReferralStatus.RmcCall}," +
       $"{ReferralStatus.ProviderAwaitingStart}";


      validData.Add(new object[] { set1, true, false, "Set 1 used" });
      validData.Add(new object[] { set2, true, false, "Set 2 used" });
      validData.Add(new object[] { set3, true, false, "Set 3 used" });
      validData.Add(new object[] { set4, false, true, "Set 4 used" });
      validData.Add(new object[] { set5, true, false, "Set 5 used" });
      validData.Add(new object[] { set6, false, true, "Set 6 used" });
      validData.Add(new object[] { set7, false, false, "Set 7 used" });
      validData.Add(new object[] { set8, false, false, "Set 8 used" });

      return validData;
    }
  }
}
