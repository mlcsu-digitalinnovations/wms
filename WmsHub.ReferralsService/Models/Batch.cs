using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.ReferralsService.Models
{
  [ExcludeFromCodeCoverage]
  public class Batch
  {
    public string FileName { get; set; }
    public List<BatchItem> Items { get; set; }
  }
}
