using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.ReferralsService.Models
{
  [ExcludeFromCodeCoverage]
  public class BatchItem
  {
    public bool AlreadyExists { get; set; }
    public string ServiceId { get; set; }
    public string Ubrn { get; set; }
    public long AttachmentId { get; set; }
    public string AttachmentFileName { get; set; }
  }
}
