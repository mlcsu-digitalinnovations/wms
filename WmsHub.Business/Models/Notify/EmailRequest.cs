using System.Collections.Generic;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.Notify;

public class EmailRequest : IEmailRequest
{
  public string ClientReference { get; set; }
  public string Email { get; set; }
  public string EmailReplyToId { get; set; }
  public Dictionary<string, dynamic> Personalisation { get; set; }
  public string SenderId { get; set; }
  public string TemplateId { get; set; }
}
