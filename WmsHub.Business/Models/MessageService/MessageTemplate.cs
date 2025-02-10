using System;
using WmsHub.Business.Enums;
using WmsHub.Business.Extensions;
using WmsHub.Business.Models.Notify;
using WmsHub.Common.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WmsHub.Business.Models.MessageService;

public class MessageTemplate : ITemplate, IValidatableObject
{
  public Guid Id { get; set; }

  public string Name { get; set; }

  public int MessageTypeValue { get; set; }

  public string ExpectedPersonalisationCsv { get; set; }

  public string[] ExpectedPersonalisationList => 
    ExpectedPersonalisationCsv.Split(",");

  public MessageType MessageType => Attribute.Type;

  public ReferralSource Source => Attribute.Source;

  public ReferralStatus Status => Attribute.Status;

  public MessageTemplates TemplateType => Name.ToEnum<MessageTemplates>();

  public MessageTemplateLookupAttribute Attribute => 
    TemplateType.GetAttributeOfType<MessageTemplateLookupAttribute>();

  public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
  {
    if (Id == Guid.Empty)
    {
      yield return new ValidationResult($"{nameof(Id)} should be supplied.");
    }
  }
}

