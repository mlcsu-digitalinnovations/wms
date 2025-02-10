using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WmsHub.Business.Exceptions;
public class MskOrganisationValidationException : Exception
{
  public Dictionary<string, string[]> ValidationResults { get; private set; }

  public MskOrganisationValidationException()
  {
  }

  public MskOrganisationValidationException(List<ValidationResult> results)
  {
    SetValidationResults(results);
  }

  public MskOrganisationValidationException(
    List<ValidationResult> results, string message)
    : base(message)
  {
    SetValidationResults(results);
  }

  public MskOrganisationValidationException(
    List<ValidationResult> results, string message, Exception inner)
    : base(message, inner)
  {
    SetValidationResults(results);
  }

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
