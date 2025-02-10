using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;

namespace WmsHub.Business.Models;

public class CompleteQuestionnaire : IValidatableObject
{
  private string _answers;

  [Required]
  [MaxLength(200)]
  public string NotificationKey { get; set; }
  [Required]
  public QuestionnaireType QuestionnaireType { get; set; }
  [Required]
  [JsonString]
  public string Answers
  {
    get => _answers;
    set => _answers = SetAnswers(value);
  }
  [EmailAddress]
  public string Email { get; private set; }
  public string FamilyName { get; private set; }
  public string GivenName { get; private set; }
  [UkMobile]
  public string Mobile { get; private set; }
  public bool ConsentToShare { get; private set; }

  private ValidationResult _validationResult = null;

  public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
  {
    if (QuestionnaireType == QuestionnaireType.None)
    {
      yield return new ValidationResult(
        $"The field {nameof(QuestionnaireType)} is required.",
        new[] { nameof(QuestionnaireType) });;
    }

    if (AreAnswersValid)
    {
      if (ConsentToShare)
      {
        if (Email == null && Mobile == null)
        {
          yield return new ValidationResult(
            $"When {nameof(ConsentToShare)} = true, one of the " +
              $"{nameof(Email)} or {nameof(Mobile)} fields is required.",
            new[] { nameof(Email), nameof(Mobile) });
        }
      }
    }
    else
    {
      yield return _validationResult;
    }
  }

  private bool AreAnswersValid => _validationResult == null;

  private string SetAnswers(string value)
  {
    try
    {
      _validationResult = null;

      object[] answersArray = JsonConvert.DeserializeObject<object[]>(value);

      if (answersArray == null || !answersArray.Any())
      {
        _validationResult = new ValidationResult(
          $"Unable to deserialise {nameof(Answers)} to an object array.",
          new[] { nameof(Answers) });
        return value;
      }

      JObject jConsentAnswer = answersArray[^1] as JObject;
      ConsentAnswer consentAnswer = jConsentAnswer.ToObject<ConsentAnswer>();

      if (consentAnswer.ConsentToShare == null)
      {
        _validationResult = new ValidationResult(
          $"The last answer of the {nameof(Answers)} array is not the " +
            $"expected consent answer.",
          new[] { nameof(Answers) });
      }
      else
      {
        if (consentAnswer.QuestionId != answersArray.Length)
        {
          _validationResult = new ValidationResult(
            "Found Answers property QuestionId to have a value of " +
              $"{consentAnswer.QuestionId} so expected to find " +
              $"{consentAnswer.QuestionId} answers but found " +
              $"{answersArray.Length}.",
            new[] { nameof(Answers) });
        }

        ConsentToShare = consentAnswer.ConsentToShare.Value;
        if (ConsentToShare)
        {
          Email = string.IsNullOrWhiteSpace(consentAnswer.Email)
            ? null
            : consentAnswer.Email;
          FamilyName = string.IsNullOrWhiteSpace(consentAnswer.FamilyName)
            ? null
            : consentAnswer.FamilyName;
          GivenName = string.IsNullOrWhiteSpace(consentAnswer.GivenName)
            ? null
            : consentAnswer.GivenName;
          Mobile = string.IsNullOrWhiteSpace(consentAnswer.Mobile)
            ? null
            : consentAnswer.Mobile;
        }
      }
      
      return value;
    }
    catch(Exception ex)
    {
      _validationResult = new ValidationResult(
        ex.Message,
        new[] { nameof(Answers) });
      return value;
    }
  }

  private class ConsentAnswer
  {
    public int QuestionId { get; set; }
    [JsonProperty("a")]
    public bool? ConsentToShare { get; set; }
    [JsonProperty("b")]
    public string Email { get; set; }
    [JsonProperty("c")]
    public string Mobile { get; set; }
    [JsonProperty("d")]
    public string GivenName { get; set; }
    [JsonProperty("e")]
    public string FamilyName { get; set; }
  }
}
