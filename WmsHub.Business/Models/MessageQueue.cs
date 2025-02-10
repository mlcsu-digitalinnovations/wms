using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using WmsHub.Business.Enums;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.Interfaces;
using WmsHub.Business.Models.Notify;
using WmsHub.Common.Extensions;
using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.Business.Models;

public class MessageQueue : IValidatableObject, IMessageQueue
{
  public MessageQueue(
    ApiKeyType apiKeyType = ApiKeyType.None,
    string clientReference = null,
    string emailTo = null,
    string emailReplyToId = null,
    string endPoint = null,
    string[] personalisationList = null,
    string mobile = null,
    Dictionary<string, dynamic> personalisations = null,
    Guid? templateId = null,
    MessageType type = MessageType.SMS,
    string linkId = null)
  {
    ApiKeyType = apiKeyType;
    ServiceUserLinkId = linkId ?? Base36Converter
      .ConvertDateTimeOffsetToBase36(DateTimeOffset.Now);
    ClientReference = clientReference;

    if (type == MessageType.Email)
    {
      EmailTo = emailTo;
      EmailReplyToId = emailReplyToId;
    }

    Endpoint = endPoint ?? type switch
    {
      MessageType.Email => MessageTemplateConstants.ENDPOINT_EMAIL,
      MessageType.SMS => MessageTemplateConstants.ENDPOINT_SMS,
      _ => ""
    };

    ExpectedPersonalisationList = personalisationList;
    if (type == MessageType.SMS)
    {
      Mobile = mobile;
    }
    
    Personalisation = personalisations;
    TemplateId = templateId ?? Guid.Empty;
    Type = type; 
  }

  public ApiKeyType ApiKeyType { get; set; }
  public string ClientReference { get; set; }
  [EmailAddress]
  public string EmailTo { get; set; }
  public string EmailReplyToId { get; set; }
  public string Endpoint { get; set; }
  public string[] ExpectedPersonalisationList { get; set; }
  public string Mobile { get; set; }
  public Dictionary<string, dynamic> Personalisation { get; set; }
  public string SenderId { get; set; }
  public string ServiceUserLinkId { get; set; }
  public Guid TemplateId { get; set; }
  public MessageType Type { get; set; }

  public EmailRequest RequestEmail => new()
  {
    ClientReference = ClientReference,
    Email = EmailTo,
    EmailReplyToId = EmailReplyToId,
    Personalisation = Personalisation,
    SenderId = SenderId,
    TemplateId = TemplateId.ToString()
  };

  public SmsPostRequest RequestText => new()
  {
    ClientReference = ClientReference,
    Mobile = Mobile,
    Personalisation = Personalisation,
    SenderId = SenderId,
    TemplateId = TemplateId.ToString()
  };

  public StringContent Content => Type switch
  {
    MessageType.Email => new StringContent(
       JsonConvert.SerializeObject(RequestEmail),
       Encoding.UTF8,
       MediaTypeNames.Application.Json),
    MessageType.SMS => new StringContent(
      JsonConvert.SerializeObject(RequestText),
      Encoding.UTF8,
      MediaTypeNames.Application.Json),
    _ => throw new ArgumentException($"{nameof(Type)} " +
      $"does not have StringContent.")
  };

  public IEnumerable<ValidationResult> Validate(
  ValidationContext validationContext)
  {
    if (Guid.TryParse(ClientReference, out Guid clientRef))
    {
      if (clientRef == Guid.Empty)
      {
        yield return new ValidationResult(
         $"{nameof(ClientReference)} cannot have an empty Guid.");
      }
    }
    else
    {
      yield return new ValidationResult(
       $"{nameof(ClientReference)} cannot be null.");
    }

    if (TemplateId == Guid.Empty)
    {
      yield return new ValidationResult(
       $"{nameof(TemplateId)} cannot have an empty Guid.");
    }

    if (Personalisation != null && !Personalisation.Any())
    {
      yield return new ValidationResult(
       $"{nameof(Personalisation)} does not contain any values.");
    }
    else
    {
      if (ExpectedPersonalisationList != null
        && ExpectedPersonalisationList.Any())
      {
        foreach (string item in ExpectedPersonalisationList)
        {
          if (!Personalisation.ContainsKey(item))
          {
            yield return new ValidationResult(
              $"{nameof(Personalisation)} does not contain expected " +
              $"key {item}.");
          }
        }
      }
    }

    if (Type == MessageType.SMS)
    {
      if (string.IsNullOrWhiteSpace(Mobile))
      {
        yield return new ValidationResult(
         $"{nameof(Mobile)} field is required.");
      }
      else if (!Mobile.IsUkMobile())
      {
        yield return new ValidationResult(
          $"{nameof(Mobile)} is not a valid UK mobile number.");
      }
    }
    else if (Type == MessageType.Email)
    {
      if (string.IsNullOrEmpty(EmailTo))
      {
        yield return new ValidationResult(
         $"{nameof(EmailTo)} field is required.");
      }
    }
  }
}
