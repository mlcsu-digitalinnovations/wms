using System;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Attributes;

namespace WmsHub.Business.Models.Notify
{
  public class TextMessageRequest : ITextMessageRequest
  {
    /// <summary>
    /// To staore a message to send
    /// </summary>
    /// <example>d1abe0f7-c454-eb11-9014-d66a6ae7d19f</example>
    [Required]
    [NotEmpty]
    public Guid ReferralId { get; set; }
    [Required]
    public string MobileNumber { get; set; }
  }
}
