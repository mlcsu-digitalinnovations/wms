using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.Notify;

public class EmailMessage: IEmailRequest
{
  [Required]
  public string ClientReference { get; set; }
  [Required, EmailAddress]
  public string Email { get; set; }
  public string EmailReplyToId { get; set; }
  public string[] ExpectedPersonalisationList { get; set; }
  [Required]
  public Dictionary<string, dynamic> Personalisation { get; set; }
  public string SenderId { get; set; }
  public DateTime Sent { get; set; }
  /// Used as the ServiceUser TextMessage link.
  /// </summary>
  public string ServiceUserLinkId { get; set; }
  [Required]
  public string TemplateId { get; set; }
}
