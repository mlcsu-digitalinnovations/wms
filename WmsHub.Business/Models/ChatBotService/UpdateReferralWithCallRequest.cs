using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Models.ChatBotService
{
  public class UpdateReferralWithCallRequest : IValidatableObject
  {
    public UpdateReferralWithCallRequest() { }

    public UpdateReferralWithCallRequest(
      Guid id,
      string outcome,
      string number,
      DateTimeOffset timestamp)
    {
      Id = id;
      Outcome = outcome;
      Number = number;
      Timestamp = timestamp;
    }

    public UpdateReferralWithCallRequest(UpdateReferralWithCallRequest request)
    {
      Id = request.Id;
      Outcome = request.Outcome;
      Number = request.Number;
      Timestamp = request.Timestamp;
    }

    public Guid Id { get; set; }

    [Required]
    [RegularExpression(Constants.REGEX_PHONE_PLUS_NUMLENGTH, 
      ErrorMessage = "The field Number is not a valid telephone number.")]
    public string Number { get; set; }

    [Required]
    public string Outcome { get; set; }

    [MaxSecondsAhead(Constants.MAX_SECONDS_API_REQUEST_AHEAD)]
    public DateTimeOffset Timestamp { get; set; }

    public IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      if (Id == default)
      {
        yield return new InvalidValidationResult(nameof(Id), Id);
      }

      if (!Outcome.TryParseToEnumName(out ChatBotCallOutcome _))
      {
        yield return new InvalidValidationResult(nameof(Outcome), Outcome);
      }

      if (Timestamp == default)
      {
        yield return new InvalidValidationResult(
          nameof(Timestamp), Timestamp);
      }
    }
  }
}