namespace WmsHub.ReferralsService.Pdf.Models;
public class RtfScanFilter
{
  /// <summary>
  /// Text description of the filter group, for logging purposes.
  /// </summary>
  public string Description { get; set; }
  /// <summary>
  /// Array of string patterns to match against in RTF files.
  /// </summary>
  public string[] Filters { get; set; }
}
