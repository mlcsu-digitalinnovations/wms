using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Validation;

namespace WmsHub.Referral.Api.Models
{
  public class SelfReferralPutRequest: ASelfReferralPutRequest, IValidatableObject
  {
    public IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      if (Id == Guid.Empty)
        yield
          return new InvalidValidationResult(nameof(Id), Id);

      if (ProviderId == Guid.Empty)
        yield 
          return new InvalidValidationResult(nameof(ProviderId), ProviderId);
    }
  }
}