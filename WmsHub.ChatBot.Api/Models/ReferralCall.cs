using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.ChatBot.Api.Models
{
  public class ReferralCall : IValidatableObject, IReferralCall
  {
    private string _outcome;

    [Required]
    [NotEmpty]
    public Guid? Id { get; set; }

    /// <summary>
    /// TODO: This is the call status update<br />
    /// Error, Caller Reached, Transferred to phone number, 
    /// Transferred to queue, Transferred to voicemail, Voicemail left, 
    /// Connected, Hung up, Engaged, Call guardian, No answer, Invalid number
    /// </summary>
    /// <example>Engaged</example>
    [Required]
    public string Outcome
    {
      get
      {
        return _outcome;
      }
      set
      {
        if (string.IsNullOrWhiteSpace(value)
          || char.IsUpper(value[0])
          || value.Length <= 1)
        {
          _outcome = value;
        }
        else
        {
          _outcome = value[0].ToString().ToUpper() + value[1..];
        }
      }
    }
    [Required]
    [MaxSecondsAhead(Constants.MAX_SECONDS_API_REQUEST_AHEAD)]
    public DateTimeOffset? Timestamp { get; set; }

    [Required]
    [RegularExpression(Constants.REGEX_PHONE_PLUS_NUMLENGTH, 
      ErrorMessage = "The field Number is not a valid telephone number.")]
    public string Number { get; set; }

    public IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      if (!Enum.IsDefined(typeof(ChatBotCallOutcome), Outcome))
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

  public class ReferralCallStart
  {
    /// <summary>
    /// Not currently used or tested, but here to provide a secure link
    /// </summary>
    [Required]
    [NotNullOrEmpty]
    public Guid? Id { get; set; }

    /// <summary>
    /// Example filters are New, ChatBotCall1, ChatBotCall2, ChatBotCall3
    /// </summary>
    /// <example>ChatBotCall1</example>
    [Required, MinLength(1), MaxLength(200)]
    public string Filter { get; set; }
  }

}