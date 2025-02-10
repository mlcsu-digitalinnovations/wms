using System.Collections.Generic;

namespace WmsHub.Business.Models.Interfaces;

public interface IEmailRequest
{
  string ClientReference { get; set; }
  string Email { get; set; }
  string EmailReplyToId { get; set; }
  Dictionary<string, dynamic> Personalisation { get; set; }
  string SenderId { get; set; }
  string TemplateId { get; set; }

}