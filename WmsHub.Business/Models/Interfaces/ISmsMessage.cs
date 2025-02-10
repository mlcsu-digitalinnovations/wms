using System;
using System.Collections.Generic;

namespace WmsHub.Business.Models.Notify;

public interface ISmsMessage
{
  string ClientReference { get; set; }
  string MobileNumber { get; set; }
  Dictionary<string, dynamic> Personalisation { get; set; }
  string Reference { get; set; }
  DateTimeOffset Sent { get; set; }
  string SmsId { get; set; }
  string TemplateId { get; set; }
  Guid? LinkedTextMessage { get; set; }
  string ServiceUserLinkId { get; set; }
  string ReferralSource { get; set; }
}