using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.ProviderService
{
  public class ServiceUserSubmissionRequestV2 : ServiceUserSubmissionRequest
  {
    public virtual ProviderRejectionReason[] ReasonList { get; set; }
    public override IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      base.Validate(validationContext);

      if (ReasonStatus == ReferralStatus.ProviderRejected)
      {
        bool IsValid = false;
        //Is Reason the rejection reason Id
        if (Guid.TryParse(Reason, out Guid id))
        {
          var foundById = ReasonList.FirstOrDefault(t => t.Id == id);
          if (foundById != null)
          {
            if (!foundById.IsActive)
            {
              yield return new ValidationResult(
                $"The {nameof(Reason)} is no longer a " +
                $"valid ProviderRejectionReason");
            }

            IsValid = true;

            Reason = foundById.Description;
          }
        }

        if (!IsValid)
        {
          //Match against title
          var foundTitle =
            ReasonList.FirstOrDefault(
              t => t.Title.ToLower() == Reason.ToLower());

          if (foundTitle == null)
          {
            yield return new ValidationResult(
              $"The {nameof(Reason)} is not a " +
              $"valid ProviderRejectionReason");
          }
          else if (!foundTitle.IsActive)
          {
            yield return new ValidationResult(
              $"The {nameof(Reason)} is no longer a " +
              $"valid ProviderRejectionReason");
          }
          else
          {
            Reason = foundTitle.Description;
          }
        }
      }
    }
  }
}