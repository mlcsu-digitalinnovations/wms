using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Models.ProviderService
{
  public class ServiceUserUpdatesRequest : IValidatableObject, IServiceUserUpdatesRequest
  {
    /// <summary>
    /// The date of the update
    /// <TypeRequirements>
    /// Rejected: Not Required,
    /// Started: Required,
    /// Update: Required,
    /// Terminated: Not Required,
    /// Completed: Required
    /// </TypeRequirements>
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// The self-reported weight of the service user at the provided date 
    /// in kgs.
    /// <summary>
    /// The date of the update
    /// <TypeRequirements>
    /// Rejected: Not Required,
    /// Started: Required at least once in array,
    /// Update: Required at least once in array,
    /// Terminated: Not Required,
    /// Completed: Required at least once in array,
    /// </TypeRequirements>
    /// </summary>
    /// </summary>
    [Range(Constants.MIN_WEIGHT_KG, Constants.MAX_WEIGHT_KG)]
    public Decimal? Weight { get; set; }

    /// <summary>
    /// The pre-selected engagement measure recorded at the provided date.
    /// <TypeRequirements>
    /// Rejected: Not Required,
    /// Started: Required at least once in array,
    /// Update: Required at least once in array,
    /// Terminated: Not Required,
    /// Completed: Required at least once in array,
    /// </TypeRequirements>
    /// </summary>
    public int? Measure { get; set; }

    /// <summary>
    /// The coaching time in minutes for the level 2 and 3 service users.
    /// <TypeRequirements>
    /// Rejected: Not Required,
    /// Started: Required at least once in array,
    /// Update: Required at least once in array,
    /// Terminated: Not Required,
    /// Completed: Required at least once in array,
    /// </TypeRequirements>
    /// </summary>
    public int? Coaching { get; set; }

    public IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
     yield break;
    }
  }
}
