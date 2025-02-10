using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using WmsHub.Business.Enums;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Models.ProviderService
{
  public class ServiceUserSubmissionRequest
    : IValidatableObject, IServiceUserSubmissionRequest
  {
    /// <summary>
    /// The Unique Booking Reference Number, the servicer user’s identifier.
    /// Is used to find the Referral
    /// </summary>
    [Required]
    [StringLength(12, MinimumLength = 12)]
    public string Ubrn { get; set; }

    /// <summary>
    /// The rejected, started, terminated, or completed date.
    /// <TypeRequirements>
    /// Rejected: Required,
    /// Started: Required,
    /// Update: Not Required,
    /// Terminated: Required,
    /// Completed: Required
    /// Contacted: Reqiured
    /// </TypeRequirements>
    /// </summary>
    public DateTimeOffset? Date { get; set; }

    /// <summary>
    /// The type of the request.
    /// </summary>
    [Required]
    public string Type { get; set; }

    [JsonIgnore]
    public UpdateType UpdateType => Type.ToLower() switch
    {
      "rejected" => UpdateType.Rejected,
      "started" => UpdateType.Started,
      "update" => UpdateType.Update,
      "terminated" => UpdateType.Terminated,
      "completed" => UpdateType.Completed,
      "accepted" => UpdateType.Accepted,
      "declined" => UpdateType.Declined,
      "contacted" => UpdateType.Contacted,
      _ => UpdateType.NotValid,
    };

    [JsonIgnore]
    public ReferralStatus? ReasonStatus => Type.ToLower() switch
    {
      "rejected" => ReferralStatus.ProviderRejected,
      "started" => ReferralStatus.ProviderStarted,
      "terminated" => ReferralStatus.ProviderTerminated,
      "completed" => ReferralStatus.ProviderCompleted,
      "accepted" => ReferralStatus.ProviderAccepted,
      "declined" => ReferralStatus.ProviderDeclinedByServiceUser,
      "contacted" => ReferralStatus.ProviderContactedServiceUser,
      _ => null
    };

    /// <summary>
    /// The reason why the service user was rejected or terminated.
    /// Saved in the Referral StatusReason
    /// <TypeRequirements>
    /// Rejected: Required,
    /// Started: Not Required,
    /// Update: Not Required,
    /// Terminated: Required,
    /// Completed: Not Required
    /// Contacted: Not Required
    /// </TypeRequirements>
    /// </summary>
    [StringLength(500)]
    public virtual string Reason { get; set; }

    /// <summary>
    /// An array of updates
    /// <TypeRequirements>
    /// Rejected: Not Required,
    /// Started: Optional,
    /// Update: Required,
    /// Terminated: Not Required,
    /// Completed: Optional
    /// Contacted: Optional
    /// </TypeRequirements>
    /// </summary>
    public IEnumerable<ServiceUserUpdatesRequest> Updates { get; set; }
      = new List<ServiceUserUpdatesRequest>();

    public virtual IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      if (string.IsNullOrWhiteSpace(Ubrn) || Ubrn.Length != 12)
      {
        yield return new InvalidValidationResult(nameof(Ubrn), Ubrn);
      }

      string pattern = Ubrn[..2].ToUpper() switch
      {
        "SR" => @"^SR[0-9]*$",
        "PR" => @"^PR[0-9]*$",
        "GR" => @"^GR[0-9]*$",
        "MS" => @"^MSK[0-9]*$",
        "GP" => @"^GP[0-9]*$",
        "EC" => @"^EC[0-9]*$",
        _ => @"^[0-9]*$",
      };

      Regex regex = new(pattern);
      if (!regex.Match(Ubrn).Success)
      {
        yield return new InvalidValidationResult(nameof(Ubrn), Ubrn);
      }

      if (UpdateType == UpdateType.NotValid)
      {
        yield return new InvalidValidationResult(nameof(Type), Type);
      }

      if (UpdateType != UpdateType.Update)
      {
        if (Date == null)
        {
          yield return new RequiredValidationResult(nameof(Date));
        }
        else if (Date.Value == DateTime.MinValue)
        {
          yield return new InvalidValidationResult(nameof(Date), Date);
        }
      }

      if (UpdateType == UpdateType.Rejected
        || UpdateType == UpdateType.Terminated
        || UpdateType == UpdateType.Declined)
      {
        if (string.IsNullOrWhiteSpace(Reason))
        {
          yield return new InvalidValidationResult(nameof(Reason), Reason);
        }

        if (Updates.Any())
        {
          yield return new InvalidValidationResult(
                nameof(Updates), "0 occurances");
        }
      }
      else
      {

        if (!Updates.Any())
        {
          if (UpdateType == UpdateType.Update)
          {
            yield return new InvalidValidationResult(nameof(Updates), Updates);
          }
        }
        else
        {
          if (UpdateType == UpdateType.Started ||
            UpdateType == UpdateType.Completed)
          {
            int nullDateCount = Updates.Count(t => t.Date == null);

            if (nullDateCount > 0)
            {
              yield return new RequiredValidationResult(
                nameof(ServiceUserUpdatesRequest.Date));
            }

            int weightCount = Updates.Count(t => t.Date != null
              && t.Weight != null);

            int measureCount = Updates.Count(t => t.Date != null
              && t.Measure != null);

            int coachCount = Updates.Count(t => t.Date != null
              && t.Coaching != null);

            if (weightCount == 0 && measureCount == 0 && coachCount == 0)
            {
              yield return new InvalidValidationResult(
                nameof(Updates),
                "0 occurances of Weight, Measure or Coaching");
            }

            if (coachCount > 0)
            {
              int maxExceededCount = Updates.Count(t => t.Date != null
                && t.Coaching != null
                && t.Coaching > 100);

              if (maxExceededCount > 0)
              {
                yield return new InvalidValidationResult(
               nameof(ServiceUserUpdatesRequest.Coaching),
                $"{maxExceededCount} occurances of Coaching exceed the " +
                $"max of 100");
              }
            }

          }
        }
      }

      foreach (ServiceUserUpdatesRequest update in Updates)
      {
        ValidateModelResult result = Validators.ValidateModel(update);
        if (result.IsValid)
        {
          continue;
        }

        foreach (ValidationResult vResult in result.Results)
        {
          yield return new ValidationResult(
            vResult.ErrorMessage, vResult.MemberNames);
        }

      }
    }
  }
}
