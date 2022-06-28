using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.ReferralService.Interop
{
  public class InteropResult
  {
    public string ErrorText { get; set; } = "No Errors";
    public bool WordError { get; set; } = false;
    public bool ExportError { get; set; } = false;
    public byte[] Data { get; set; }
  }
}
