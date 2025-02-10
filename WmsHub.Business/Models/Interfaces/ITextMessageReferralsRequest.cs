using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models.Notify
{
  public interface ITextMessageReferralsRequest
  {
    string[] Referrals { get; set; }
    string TemplateId { get; set; }
    string Status { get; set; }

    IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext);
  }
}