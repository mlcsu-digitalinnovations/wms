using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WmsHub.ReferralsService.Models
{
  [ExcludeFromCodeCoverage]
  public class AttachmentList
  {
    public string Status { get; set; }
    public DateTimeOffset Indexed { get; set; }
    public List<AttachmentContainer> Content { get; set; }
  }
}
