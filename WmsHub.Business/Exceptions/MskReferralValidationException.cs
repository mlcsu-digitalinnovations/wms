using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class MskReferralValidationException : AReferralValidationException
  {
    public MskReferralValidationException() : base()
    { }

    public MskReferralValidationException(List<ValidationResult> results)
      : base(results)
    { }

    public MskReferralValidationException(
      string message, List<ValidationResult> results)
      : base(results, message)
    { }

    public MskReferralValidationException(
      string message, Exception inner, List<ValidationResult> results)
      : base(results, message, inner)
    { }
  }
}