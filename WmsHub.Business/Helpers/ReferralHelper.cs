using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Helpers;

public static class ReferralHelper
{
  public static void CheckMatchingReferralsIfReEntryIsAllowed(
    List<Entities.Referral> referrals,
    string matchType = "NHS number")
  {
    if (referrals?.Count > 0)
    {
      List<Entities.Referral> cancelledOrComplete = referrals
        .Where(r =>
          r.Status == ReferralStatus.CancelledByEreferrals.ToString()
          || r.Status == ReferralStatus.Complete.ToString()
          || r.Status == ReferralStatus.Cancelled.ToString()
        )
        .ToList();

      if (cancelledOrComplete.Count == referrals.Count)
      {
        if (cancelledOrComplete.Any(r => r.ProviderId != null))
        {
          Entities.Referral latestReferralForComparison;
          DateTimeOffset? latestResponseReEntryDate;
          string latestResponseReEntryDateString;

          if (cancelledOrComplete.All(r => r.DateStartedProgramme == null))
          {
            latestReferralForComparison = cancelledOrComplete
              .Where(r => r.ProviderId != null)
              .MaxBy(r => r.DateOfProviderSelection);

            if (!latestReferralForComparison.DateOfProviderSelection.HasValue)
            {
              throw new InvalidOperationException("The previous referral " +
                $"(UBRN {latestReferralForComparison.Ubrn}) has a selected " +
                "provider without a matching date of provider selection.");
            }

            latestResponseReEntryDate = latestReferralForComparison
              .DateOfProviderSelection
              .Value
              .AddDays(Constants.MIN_DAYS_SINCE_DATEOFPROVIDERSELECTION);

            if (DateTimeOffset.Now.Date
              <= latestResponseReEntryDate.Value.Date)
            {
              latestResponseReEntryDateString = latestResponseReEntryDate
                .Value
                .Date
                .AddDays(1)
                .ToString("yyyy-MM-dd");

              throw new ReferralNotUniqueException(
                $"Referral can be created from {latestResponseReEntryDateString} as an " +
                $"existing referral for this {matchType} (UBRN " +
                $"{latestReferralForComparison.Ubrn}) selected a provider but did not start " +
                $"the programme.",
                new List<string> { latestReferralForComparison.Ubrn });
            }
          }
          else
          {
            latestReferralForComparison = cancelledOrComplete
              .Where(r => r.ProviderId != null)
              .Where(r => r.DateStartedProgramme != null)
              .MaxBy(r => r.DateStartedProgramme);

            latestResponseReEntryDate = latestReferralForComparison
              .DateStartedProgramme
              .Value
              .AddDays(Constants.MIN_DAYS_SINCE_DATESTARTEDPROGRAMME);

            if (DateTimeOffset.Now.Date
              <= latestResponseReEntryDate.Value.Date)
            {
              latestResponseReEntryDateString = latestResponseReEntryDate
                .Value
                .Date
                .AddDays(1)
                .ToString("yyyy-MM-dd");

              throw new ReferralNotUniqueException(
                $"Referral can be created from {latestResponseReEntryDateString} as an " +
                $"existing referral for this {matchType} (UBRN " +
                $"{latestReferralForComparison.Ubrn}) started the programme.",
                new List<string> { latestReferralForComparison.Ubrn });
            }
          }
        }
      }
      else
      {
        List<string> inProgressReferralUbrns = referrals
          .Where(r =>
            r.Status != ReferralStatus.CancelledByEreferrals.ToString()
            && r.Status != ReferralStatus.Complete.ToString()
            && r.Status != ReferralStatus.Cancelled.ToString())
          .Select(r => r.Ubrn)
          .ToList();

        StringBuilder ubrnStringBuilder = new();
        ubrnStringBuilder.Append("(UBRN ");
        ubrnStringBuilder.Append(string.Join(", ", inProgressReferralUbrns));
        ubrnStringBuilder.Append(").");

        throw new ReferralNotUniqueException("Referral cannot be created " +
          "because there are in progress referrals with the same " +
          $"{matchType}: {ubrnStringBuilder}",
          inProgressReferralUbrns);
      }
    }
  }
}
