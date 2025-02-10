using System.ComponentModel.DataAnnotations;
using System;

namespace WmsHub.Common.Api.Models;

public class ReferralInvalidAttachmentPost : ReferralPostBase
{
  //[Required] annotation temporarily removed while correct usage of ReferralMissingAttachmentPost implemented
  public DateTimeOffset? MostRecentAttachmentDate { get; set; }

  //[Required] annotation temporarily removed while correct usage of ReferralMissingAttachmentPost implemented
  public string ReferralAttachmentId { get; set; }
}