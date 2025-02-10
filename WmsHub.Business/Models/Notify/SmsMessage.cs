using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models.Notify;

public class SmsMessage : ISmsMessage
{
  public string ClientReference { get; set; }
  [Phone(ErrorMessage = "The MobileNumber field is invalid.")]
  [MinLength(9, ErrorMessage = "The MobileNumber field is too short.")]
  [Required]
  public string MobileNumber { get; set; }
  [Required]
  public string TemplateId { get; set; }
  [Required]
  public Dictionary<string, dynamic> Personalisation { get; set; }
  public DateTimeOffset Sent { get; set; }
  public string SmsId { get; set; }
  public string Reference { get; set; }
  public Guid? LinkedTextMessage { get; set; }
  public string ServiceUserLinkId { get; set; }
  public string ReferralSource { get; set; }
  public string ReferralStatus { get; set; }
}
