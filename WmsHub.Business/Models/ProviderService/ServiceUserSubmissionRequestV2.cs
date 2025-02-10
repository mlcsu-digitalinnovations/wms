using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ReferralStatusReason;

namespace WmsHub.Business.Models.ProviderService
{
  public class ServiceUserSubmissionRequestV2 : ServiceUserSubmissionRequest
  {
    public virtual ReferralStatusReason.ReferralStatusReason[] ReasonList { get; set; }

    public override IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      base.Validate(validationContext);

      if (ReasonStatus is ReferralStatus.ProviderRejected
        or ReferralStatus.ProviderTerminated
        or ReferralStatus.ProviderDeclinedByServiceUser)
      {
        // Is the Reason provided a GUID? If so try to match against the 
        // existing provider rejection reason list's ids.
        bool reasonExists = false;

        if (Guid.TryParse(Reason, out Guid id))
        {
          ReferralStatusReason.ReferralStatusReason existingReason = ReasonList
            .SingleOrDefault(t => t.Id == id);

          if (existingReason != null)
          {
            if (!existingReason.IsActive)
            {
              yield return new ValidationResult(
                $"'{Reason}' is no longer a valid " +
                $"{ReasonStatus} reason.");
            }

            reasonExists = true;

            Reason = existingReason.Description;
          }
        }

        if (!reasonExists)
        {
          // Try to match against the existing provider rejection reason list's
          // descriptions.
          ReferralStatusReason.ReferralStatusReason existingReason = ReasonList
            .SingleOrDefault(x => x.Description
              .Equals(base.Reason, StringComparison.OrdinalIgnoreCase));

          if (existingReason == null)
          {
            yield return new ValidationResult(
              $"'{Reason}' is not a valid {ReasonStatus} reason.");
          }
          else if (!existingReason.IsActive)
          {
            yield return new ValidationResult(
              $"'{Reason}' is no longer a valid {ReasonStatus} reason.");
          }
          else
          {
            Reason = existingReason.Description;
          }
        }
      }
    }
  }
}