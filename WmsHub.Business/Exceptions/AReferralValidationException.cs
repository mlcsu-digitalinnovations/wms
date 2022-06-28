using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace WmsHub.Business.Exceptions
{
  public abstract class AReferralValidationException : Exception
  {
    public Dictionary<string, string[]> ValidationResults { get; private set; }

    protected AReferralValidationException()
    {
    }

    protected AReferralValidationException(List<ValidationResult> results)
    {
      SetValidationResults(results);
    }

    public AReferralValidationException(
      List<ValidationResult> results, string message) 
      : base(message)
    {
      SetValidationResults(results);
    }

    public AReferralValidationException(
      List<ValidationResult> results, string message, Exception inner)
      : base(message, inner)
    {
      SetValidationResults(results);
    }

    public AReferralValidationException(
      SerializationInfo info, StreamingContext context) 
      : base(info, context)
    { }

    protected void SetValidationResults(List<ValidationResult> results)
    {
      ValidationResults = results
        .SelectMany(
          result => result.MemberNames,
          (result, memberName) => new { result, memberName })
        .GroupBy(
          resultAndMemberName => resultAndMemberName.memberName,
          resultAndMemberName => resultAndMemberName.result.ErrorMessage)
        .Select(ErrorMessagesGroup => new
        {
          MemberName = ErrorMessagesGroup.Key,
          Errors = ErrorMessagesGroup.ToArray()
        })
        .ToDictionary(r => r.MemberName, r => r.Errors);
    }
  }
}
