using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.ReferralsService.Pdf.Models;
public class RtfPreprocessorConfig
{
  /// <summary>
  /// Determines whether filters are active.
  /// </summary>
  public bool ApplyRtfScanFilter { get; set; } = true;
  /// <summary>
  /// Array of filters to be applied.
  /// </summary>
  public RtfScanFilter[] RtfScanFilters { get; set; }
  /// <summary>
  /// Minimum buffer size in bytes. Must be 2 or greater and should be an even number. Defaults to 
  /// 256.
  /// </summary>
  [Range(2, int.MaxValue)]
  public int MinimumRtfScanBufferSize { get; set; } = 256;
}
