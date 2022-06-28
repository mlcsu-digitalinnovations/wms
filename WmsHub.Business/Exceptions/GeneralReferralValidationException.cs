using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  [Serializable]
  public class GeneralReferralValidationException : AReferralValidationException
  {
    public GeneralReferralValidationException()
      : base()
    { }

    public GeneralReferralValidationException(List<ValidationResult> results)
      : base(results)
    { }

    public GeneralReferralValidationException(
      string message, List<ValidationResult> results)
      : base(results, message)
    { }

    public GeneralReferralValidationException(
      string message, Exception inner, List<ValidationResult> results)
      : base(results, message, inner)
    { }

    protected GeneralReferralValidationException(
      SerializationInfo info, StreamingContext context)
      : base(info, context)
    { }
  }
}