using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Extensions;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.MessageService;

public class MessageOptions : IMessageOptions, IValidatableObject
{
  public const string SECTIONKEY = "MessageServiceSettings";

  public static Enums.ReferralStatus StatusFilterFlag { get; set; }

  public static List<string> QueueStatus => 
    StatusFilterFlag.ToString()
    .Replace(" ", "")
    .Split(",")
    .ToList();

  public string ReplyToId { get; set; }

  public string SenderId { get; set; }

  public string ServiceUserRmcEndpoint { get; set; }

  /// <inheritdoc/>
  public string TemplateJson { get; set; }

  public List<MessageTemplate> Templates => 
    Newtonsoft.Json.JsonConvert
    .DeserializeObject<List<MessageTemplate>>(TemplateJson);

  public MessageTemplate GetTemplate(
    Enums.MessageType messageType, 
    Enums.ReferralStatus status, 
    Enums.ReferralSource source)
  {
    MessageTemplate templates = Templates
      .Where(t => t.MessageType == messageType)
      .Where(t => t.Source.HasFlag(source))
      .Where(t => t.Status.HasFlag(status))
      .SingleOrDefault() ?? throw new TemplateNotFoundException(
        messageType.ToString(), 
        status.ToString(), 
        source.ToString());

    return templates;
  }

  public MessageTemplate GetTemplateById(Guid templateId) => 
    Templates.SingleOrDefault(t => t.Id == templateId);

  public MessageTemplate GetTemplateByName(string templateName)
  {
    MessageTemplate template = Templates
      .Where(t => t.Name == templateName)
      .SingleOrDefault() ?? throw new TemplateNotFoundException(
        $"{nameof(templateName)} is not in the list of templates.");

    return template;
  }

  public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
  {
    if (string.IsNullOrWhiteSpace(TemplateJson))
    {
      yield return new ValidationResult(
        $"{nameof(TemplateJson)} cannot be null or empty.");
    }
    else
    {
      if (!TemplateJson.TryDeserializeObject(out List<MessageTemplate> _))
      {
        yield return new ValidationResult(
          $"{nameof(TemplateJson)} must be a valid Json.");
      }
    }
  }
}

