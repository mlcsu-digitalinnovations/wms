using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class PharmacyReferralValidationException
    : AReferralValidationException
  {

    public PharmacyReferralValidationException() 
      : base() 
    { }

    public PharmacyReferralValidationException(List<ValidationResult> results)
      : base(results)
    { }

    public PharmacyReferralValidationException(
      string message, List<ValidationResult> results)
      : base(results, message)
    { }

    public PharmacyReferralValidationException(
      string message, Exception inner, List<ValidationResult> results)
      : base(results, message, inner)
    { }

    protected PharmacyReferralValidationException(
      SerializationInfo info, StreamingContext context)
      : base(info, context)
    { }

  }
}