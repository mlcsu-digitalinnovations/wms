using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models
{
  public interface IReferralExceptionUpdate : IReferralCreateBase
  {
    CreateReferralException ExceptionType { get; set; }
    DateTimeOffset? MostRecentAttachmentDate { get; set; }
    string NhsNumberAttachment { get; set; }
    string NhsNumberWorkList { get; set; }
    string ReferralAttachmentId { get; set; }
    string ReferralSource { get; set; }
  }
}