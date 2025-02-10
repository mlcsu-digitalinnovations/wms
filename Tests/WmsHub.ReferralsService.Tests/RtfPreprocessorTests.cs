using FluentAssertions;
using Moq;
using System;
using System.IO;
using WmsHub.ReferralsService.Pdf;
using WmsHub.ReferralsService.Pdf.Models;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.ReferralsService.Tests;
public class RtfPreprocessorTests : IDisposable
{
  protected const string FILTER_ACTIVITY_TEMPLATE = "The RTF scan filter is {0}. " +
    "This is determined by the {1} setting";
  private readonly SerilogLoggerMock _logger;

  public RtfPreprocessorTests()
  {
    _logger = new();
  }

  public void Dispose()
  {
    _logger.Messages.Clear();
    _logger.Exceptions.Clear();
    GC.SuppressFinalize(this);
  }

  public class ScanRtfFileTests : RtfPreprocessorTests
  {
    private const string MATCHING_FILTER = "\\\\d \"c:/apps";
    private const string NON_MATCHING_FILTER = "nonmatchingfilter";

    private const string FAILURE_LOG_TEMPLATE = "RTF filter failed - contains {0}.";
    private const string SUCCESS_LOG_TEXT = "RTF filter passed.";

    private readonly string _validTemplateFilePath;
    private readonly string _invalidTemplateFilePath;

    private readonly RtfScanFilter _matchingRtfScanFilter;
    private readonly RtfScanFilter _nonMatchingRtfScanFilter;

    public ScanRtfFileTests() : base()
    {
      _matchingRtfScanFilter = new()
      {
        Description = "Matching",
        Filters = new[] { MATCHING_FILTER }
      };

      _nonMatchingRtfScanFilter = new()
      {
        Description = "Non-matching",
        Filters = new[] { NON_MATCHING_FILTER }
      };

      string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
      string baseFilePath = Path.GetDirectoryName(assemblyLocation) + "/Files/";
      _validTemplateFilePath = baseFilePath + "RtfPreprocessorTests_ValidTemplate.rtf";
      _invalidTemplateFilePath = baseFilePath + "RtfPreprocessorTests_InvalidTemplate.rtf";
    }

    [Fact]
    public void FilterActive_NoMatches_LogsAndReturnsZero()
    {
      // Arrange.
      MemoryStream memoryStream = new(File.ReadAllBytes(_validTemplateFilePath));

      RtfPreprocessorConfig configuration = new()
      {
        ApplyRtfScanFilter = true,
        RtfScanFilters = new[] { _nonMatchingRtfScanFilter },
        MinimumRtfScanBufferSize = 1024
      };

      RtfPreprocessor rtfPreprocessor = new(configuration, _logger);

      string expectedActivityLog = string.Format(
        FILTER_ACTIVITY_TEMPLATE,
        "active",
        nameof(configuration.ApplyRtfScanFilter));

      int expectedOutput = 0;

      // Act.
      int output = rtfPreprocessor.ScanRtfFile(memoryStream);

      // Assert.
      _logger.Messages.Should().Contain(expectedActivityLog).And.Contain(SUCCESS_LOG_TEXT);
      output.Should().Be(expectedOutput);
    }

    [Fact]
    public void FilterActive_Match_LogsAndReturnsTwo()
    {
      // Arrange.
      MemoryStream memoryStream = new(File.ReadAllBytes(_invalidTemplateFilePath));

      RtfPreprocessorConfig configuration = new()
      {
        ApplyRtfScanFilter = true,
        RtfScanFilters = new[] { _matchingRtfScanFilter },
        MinimumRtfScanBufferSize = 1024
      };

      RtfPreprocessor rtfPreprocessor = new(configuration, _logger);

      string expectedActivityLog = string.Format(
        FILTER_ACTIVITY_TEMPLATE,
        "active",
        nameof(configuration.ApplyRtfScanFilter));

      string expectedFailureLog = string.Format(FAILURE_LOG_TEMPLATE,
        _matchingRtfScanFilter.Description);

      int expectedOutput = 2;

      // Act.
      int output = rtfPreprocessor.ScanRtfFile(memoryStream);

      // Assert.
      _logger.Messages.Should().Contain(expectedActivityLog).And.Contain(expectedFailureLog);
      output.Should().Be(expectedOutput);
    }

    [Fact]
    public void FilterInactive_LogsAndReturnsZero()
    {
      // Arrange.
      RtfPreprocessorConfig configuration = new()
      {
        ApplyRtfScanFilter = false,
        RtfScanFilters = new[] { _matchingRtfScanFilter },
        MinimumRtfScanBufferSize = 1024
      };

      RtfPreprocessor rtfPreprocessor = new(configuration, _logger);

      string expectedActivityLog = string.Format(
        FILTER_ACTIVITY_TEMPLATE,
        "inactive",
        nameof(configuration.ApplyRtfScanFilter));

      int expectedOutput = 0;

      // Act.
      int output = rtfPreprocessor.ScanRtfFile(It.IsAny<MemoryStream>());

      // Assert.
      _logger.Messages.Should().Contain(expectedActivityLog);
      output.Should().Be(expectedOutput);
    }
  }
}
