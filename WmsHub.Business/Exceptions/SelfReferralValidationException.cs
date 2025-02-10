using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class SelfReferralValidationException : AReferralValidationException
  {

    public SelfReferralValidationException()
      : base()
    { }

    public SelfReferralValidationException(List<ValidationResult> results)
      : base(results)
    { }

    public SelfReferralValidationException(
      string message, List<ValidationResult> results)
      : base(results, message)
    { }

    public SelfReferralValidationException(
      string message, Exception inner, List<ValidationResult> results)
      : base(results, message, inner)
    { }
  }
}
