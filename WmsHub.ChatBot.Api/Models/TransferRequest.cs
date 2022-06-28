using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;
using System.Globalization;

namespace WmsHub.ChatBot.Api.Models
{
  public class TransferRequest : IValidatableObject, ITransferRequest
  {
    private string _outcome;

    [Required]
    public string TransferOutcome
    {
      get
      {
        return _outcome;
      }
      set
      {
        TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
        _outcome = ti.ToTitleCase(value);
      }
    } //"TransferringToRmc"

    [Required]
    [MaxSecondsAhead(Constants.MAX_SECONDS_API_REQUEST_AHEAD)]
    public DateTimeOffset? Timestamp { get; set; } //"2021-02-24T08:37:42.974Z"

    [Required]
    [RegularExpression(Constants.REGEX_PHONE_PLUS_NUMLENGTH, 
      ErrorMessage = "The field Number is not a valid telephone number.")]
    public string Number { get; set; } //+447747807833

    public IEnumerable<ValidationResult> Validate(
     ValidationContext validationContext)
    {
      if (!Enum.IsDefined(typeof(ChatBotCallOutcome), TransferOutcome))
      {
        yield return new InvalidValidationResult(nameof(TransferOutcome),
          TransferOutcome);
      }

      if (Timestamp == default)
      {
        yield return new InvalidValidationResult(
          nameof(Timestamp), Timestamp);
      }
    }
  }
}

