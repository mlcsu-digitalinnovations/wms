using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WmsHub.ReferralsService.Pdf.Models;

namespace WmsHub.ReferralsService.Pdf;
public class RtfPreprocessor
{
  private readonly RtfPreprocessorConfig _configuration;
  private readonly ILogger _logger;
  private readonly int _minimumBuffer;

  public RtfPreprocessor(RtfPreprocessorConfig configuration, ILogger logger)
  {
    _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // Ensure _minimumBuffer is even and positive to prevent inconsistencies when dividing by 2.
    _minimumBuffer = _configuration.MinimumRtfScanBufferSize % 2 == 0 ?
      _configuration.MinimumRtfScanBufferSize : _configuration.MinimumRtfScanBufferSize + 1;

    _logger.Information("The RTF scan filter is {Activity}. This is determined by the " +
      "{SettingName} setting",
      _configuration.ApplyRtfScanFilter ? "active" : "inactive",
      nameof(_configuration.ApplyRtfScanFilter));
  }

  /// <summary>
  /// Scans a MemoryStream for a set of filter strings as specified in 
  /// ReferralsService.Console.AppSettings.Data.ParserConfig.RtfPreprocessorConfig.RtfScanFilters.
  /// Logs any matches and returns the number of matches found. If the ApplyRtfScanFilter setting 
  /// is false, this method will always return 0.
  /// </summary>
  /// <param name="memoryStream">A MemoryStream of the RTF file to be scanned.</param>
  /// <returns>The number of filter matches found.</returns>
  /// <exception cref="ArgumentNullException"></exception>
  public int ScanRtfFile(MemoryStream memoryStream)
  {
    if (!_configuration.ApplyRtfScanFilter)
    {
      return 0;
    }

    if (memoryStream == null || memoryStream.Length == 0)
    {
      throw new ArgumentNullException(nameof(memoryStream));
    }

    int longestFilterChars = _configuration.RtfScanFilters.Max(f => f.Filters.Max(x => x.Length));
    int bufferSize = Math.Max(longestFilterChars * 2, _minimumBuffer);
    int offset = 0;
    int position = 0;
    int readLength = bufferSize;
    byte[] buffer = new byte[bufferSize];

    List<RtfScanFilter> filtersMatched = [];

    while (position < memoryStream.Length)
    {
      memoryStream.Read(buffer, offset, readLength);
      ReadOnlySpan<byte> readOnlySpan = new(buffer);

      foreach (RtfScanFilter rtfScanFilter in _configuration.RtfScanFilters)
      {
        foreach (string filter in rtfScanFilter.Filters)
        {
          if (readOnlySpan.IndexOf(Encoding.ASCII.GetBytes(filter)) > (readLength - filter.Length))
          {
            filtersMatched.Add(rtfScanFilter);
          }
        }
      }

      position += readLength;
      readLength = bufferSize / 2;

      Array.Copy(buffer, readLength, buffer, 0, readLength);
      offset = bufferSize / 2;
    }

    if (filtersMatched.Count != 0)
    {
      _logger.Warning("RTF filter failed - contains {Description}.",
        string.Join(", ", filtersMatched.Select(f => f.Description).Distinct()));
    }
    else
    {
      _logger.Information("RTF filter passed.");
    }

    return filtersMatched.Count;
  }
}
