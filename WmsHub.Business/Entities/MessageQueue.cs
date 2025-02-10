using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Common.Extensions;

using static WmsHub.Common.Helpers.RegexUtilities;

namespace WmsHub.Business.Entities;

public class MessageQueue : MessageQueueBase, IMessageQueue, IValidatableObject
{
  public MessageQueue() 
  { }

  public MessageQueue(ApiKeyType apiKeyType, 
    string password, 
    Dictionary<string, dynamic> personalisation, 
    Guid principalId,
    string emailAddress, 
    Guid templateId, 
    MessageType type)
  {
    string personalisationJson = JsonConvert.SerializeObject(personalisation);

    ApiKeyType = apiKeyType;
    ServiceUserLinkId = password;
    IsActive = true;
    PersonalisationJson = personalisationJson;
    ReferralId = principalId;
    SendTo = emailAddress;
    TemplateId = templateId;
    Type = type;
  }

  public Dictionary<string, dynamic> Personalisations
  {
    get
    {
      if (string.IsNullOrWhiteSpace(PersonalisationJson))
      {
        throw new ArgumentNullException(nameof(PersonalisationJson));
      }

      Dictionary<string, dynamic> personalisations = JsonConvert
        .DeserializeObject<Dictionary<string, dynamic>>(
        PersonalisationJson);
      return personalisations;
    }
  }
    

  public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
  {
    if (TemplateId == System.Guid.Empty)
    {
      yield return new ValidationResult(
         $"{nameof(TemplateId)} cannot have an empty Guid.");
    }

    if (ReferralId == System.Guid.Empty)
    {
      yield return new ValidationResult(
         $"{nameof(ReferralId)} cannot have an empty Guid.");
    }

    if (Type == Enums.MessageType.Email)
    {
      if (!IsValidEmail(SendTo))
      {
        yield return new ValidationResult(
         $"{nameof(SendTo)} is not a valid Email address.");
      }
    }
    else if (Type== Enums.MessageType.SMS)
    {
      if (!SendTo.IsUkMobile())
      {
        yield return new ValidationResult(
         $"{nameof(SendTo)} is not a valid UK Mobile number.");
      }
    }
    else
    {
      yield return new ValidationResult(
        $"{nameof(Type)} is not implemented.");
    }

    if (!string.IsNullOrWhiteSpace(PersonalisationJson))
    {
      foreach(KeyValuePair<string, dynamic> item in Personalisations)
      {
        if (string.IsNullOrWhiteSpace(item.Value.ToString()))
        {
          yield return new ValidationResult(
         $"{nameof(Personalisations)} for referral {ReferralId} cannot have " +
         $"key {item.Key} with empty value.");
        }
      }
    }
    
  }
}
