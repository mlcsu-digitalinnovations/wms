using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.BusinessIntelligence;

public class BusinessIntelligenceOptions: IValidatableObject
{
  public const string SectionKey = "BusinessIntelligenceOptions";

  public int DaysBetweenTraces { get; set; } = 7;
  public string[] TraceIpWhitelist { get; set; } = { "127.0.0.1" };
  public bool IsTraceIpWhitelistEnabled { get; set; } = true;

  public virtual string ProviderSubmissionEndedStatusesValue { get; set; }

  /// <summary>
  /// Default flag has single
  /// </summary>
  public virtual ReferralStatus? ProviderSubmissionEndedStatuses
  {
    get
    {
      string[] values = ProviderSubmissionEndedStatusesValue.Split(',');
      ReferralStatus? statusFlag = null;

      foreach (string value in values)
      {
        if (Enum.TryParse(value, out ReferralStatus status))
        {
          if (statusFlag == null)
          {
            statusFlag = status;
          }
          else
          {
            statusFlag |= status;
          }
        }
      }

      return statusFlag;
    }
  }

  public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
  {
    if (string.IsNullOrEmpty(ProviderSubmissionEndedStatusesValue))
    {
      yield return new ValidationResult(
        $"{nameof(ProviderSubmissionEndedStatusesValue)} cannot be " +
        $"null or empty.");
    }
    else
    {
      if (ProviderSubmissionEndedStatuses == null)
      {
        yield return new ValidationResult(
        $"{nameof(ProviderSubmissionEndedStatuses)} cannot be null or empty.");
      }

      string[] values = ProviderSubmissionEndedStatusesValue.Split(",");
      int count = Enum.GetValues(typeof(ReferralStatus)).Cast<ReferralStatus>()
        .Count(flag => ProviderSubmissionEndedStatuses.Value.HasFlag(flag));
      // add 1 as Exception is flag 0 and always gets matched
      if (values.Length + 1 != count )
      {
        yield return new ValidationResult(
        $"{nameof(ProviderSubmissionEndedStatuses)} has a different number " +
        $"of values then in the list provided.");
      }
    }
  }
}
