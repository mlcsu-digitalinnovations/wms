using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.Notify;

namespace WmsHub.Business.Models.Interfaces;

public interface IMessageQueue
{
  ApiKeyType ApiKeyType { get; set; }
  string ClientReference { get; set; }
  StringContent Content { get; }
  string EmailReplyToId { get; set; }
  string EmailTo { get; set; }
  string Endpoint { get; set; }
  string[] ExpectedPersonalisationList { get; set; }
  string Mobile { get; set; }
  Dictionary<string, dynamic> Personalisation { get; set; }
  EmailRequest RequestEmail { get; }
  SmsPostRequest RequestText { get; }
  string SenderId { get; set; }
  string ServiceUserLinkId { get; set; }
  Guid TemplateId { get; set; }
  MessageType Type { get; set; }

  IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
}