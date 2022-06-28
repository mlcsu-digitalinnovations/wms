using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ProviderService
{
  public interface IServiceUserSubmissionRequest
  {
    /// <summary>
    /// The Unique Booking Reference Number, the servicer user’s identifier.
    /// Is used to find the Referral
    /// </summary>
    string Ubrn { get; set; }

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
    DateTimeOffset? Date { get; set; }

    /// <summary>
    /// The type of the request.
    /// </summary>
    string Type { get; set; }

    UpdateType UpdateType { get; }
    ReferralStatus? ReasonStatus { get; }

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
    string Reason { get; set; }

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
    IEnumerable<ServiceUserUpdatesRequest> Updates { get; set; }

    IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext);
  }
}