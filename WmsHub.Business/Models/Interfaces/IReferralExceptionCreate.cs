using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.Interfaces;

public interface IReferralExceptionCreate : IReferralCreateBase
{
  CreateReferralException ExceptionType { get; set; }
  DateTimeOffset? MostRecentAttachmentDate { get; set; }
  string NhsNumberAttachment { get; set; }
  string NhsNumberWorkList { get; set; }
  string ReferralAttachmentId { get; set; }
  string ReferralSource { get; set; }
  string ServiceId { get; set; }
  Common.Enums.SourceSystem? SourceSystem { get; set; }
  decimal? DocumentVersion { get; set; }
}