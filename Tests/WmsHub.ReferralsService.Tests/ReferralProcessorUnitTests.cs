using Xunit;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Serilog;
using WmsHub.Common.Api.Models;
using WmsHub.ReferralsService.Interfaces;
using WmsHub.ReferralsService.Models.Configuration;
using WmsHub.ReferralsService.Models.Results;
using WmsHub.Common.Models;
using WmsHub.ReferralsService.Pdf;
using FluentAssertions.Execution;
using WmsHub.Common.Helpers;
using System.Text;
using WmsHub.Common.Interface;
using System.Collections.Generic;
using WmsHub.ReferralsService.Models;
using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.ReferralsService.Tests;

public class ReferralProcessorUnitTests
{
  protected class MockReferralProcessor : ReferralProcessor
  {
    public MockReferralProcessor(
      IReferralsDataProvider dataProvider,
      ISmartCardAuthentictor smartCardAuthentictor,
      Config configuration, ILogger log, ILogger auditLog)
      : base(dataProvider, smartCardAuthentictor, configuration, log, auditLog)
    {}

    public async Task<bool> MockProcessNewReferral(
      ErsWorkListEntry workListEntry,
      bool reportOnly,
      bool showDiagnostics,
      string attachmentId = null)
    {
      return await base.ProcessNewReferral(
        workListEntry,
        reportOnly,
        showDiagnostics,
        attachmentId);
    }
  }

  private MockReferralProcessor _classToTest;

  private readonly Mock<IReferralsDataProvider> _mockProvider =
    new Mock<IReferralsDataProvider>();

  private readonly Mock<ISmartCardAuthentictor> _mockSmartCardAuthenticator =
    new Mock<ISmartCardAuthentictor>();

  private readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();
  private Config _config;

  public ReferralProcessorUnitTests()
  {

    _config = new Config
    {
      Data = new DataConfiguration()
      {
        ExcludedFiles = new() { "obs" }
      }
    };

    _mockLogger
      .Setup(t => t.Information(
        It.IsAny<string>()))
      .Verifiable();

    _classToTest = new MockReferralProcessor(
      _mockProvider.Object,
      _mockSmartCardAuthenticator.Object,
      _config,
      _mockLogger.Object,
      _mockLogger.Object);

  }

  public class ProcessNewReferralsTest : ReferralProcessorUnitTests, IDisposable
  {
    
    private ErsReferralResult _ersReferralResult;
    private string _expectedTestErrorMessage = "There were test errors.";
    private Mock<ReferralAttachmentPdfProcessor> _mockPdfProcessor;
    private string _nhsNumber;
    private readonly ReferralPost _referralPost;
    private readonly string _serviceIdentifier = "ERS";
    private readonly string _ubrn;
    private readonly AvailableActionResult _availableActionResult = new()
    {
      Actions = new AvailableActions()
      {
        Entry = new List<AvailableActionEntry>()
        {
          new AvailableActionEntry()
          {
            Resource = new AvailableActionResource()
            {
              Code = new AvailableActionCode()
              {
                Coding = new List<AvailableActionCodingItem>()
                {
                  new AvailableActionCodingItem()
                  {
                    Code = Enums.ReferralAction
                      .RECORD_REVIEW_OUTCOME.ToString()
                  }
                }
              }
            }
          }
        }
      },
      Success = true
    };

    public ProcessNewReferralsTest()
    {
      _nhsNumber = Generators.GenerateNhsNumber(new Random());
      _ubrn = Generators.GenerateUbrnGp(new Random());
      _referralPost = new ReferralPost
      {
        NhsNumber = _nhsNumber,
        Ubrn = _ubrn,
        CriDocument = null,
        CriLastUpdated = null
      };
      
      _mockPdfProcessor = new Mock<ReferralAttachmentPdfProcessor>(
        _mockLogger.Object,
        _mockLogger.Object,
        null,
        null,
        null);
      _mockPdfProcessor
        .Setup(t => t.GenerateReferralCreationObject(
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>()))
        .Returns(_referralPost);
      _ersReferralResult = new ErsReferralResult
      {
        AttachmentId = "1",
        ErsReferral = new ErsReferral()
        {
          Id = _ubrn,
          Meta = new() { versionId = "1" }
        },
        InteropErrors = false,
        MostRecentAttachmentDate = DateTimeOffset.Now,
        NoValidAttachmentFound = false,
        Pdf = _mockPdfProcessor.Object,
        ServiceIdentifier = _serviceIdentifier,
        Success = false,
        Ubrn = _ubrn,
      };     
    }

    public void Dispose()
    {
      _mockProvider.Reset();
      _mockSmartCardAuthenticator.Reset();
      _mockLogger.Reset();
      _ersReferralResult = null;
    }

    [Fact]
    public async Task RegistrationRecord_IsNull_ReturnFalse_WithDebugLog()
    {
      // Arrange.
      ErsWorkListEntry workListEntry = new ErsWorkListEntry();
      workListEntry.Item = new ERSWorkListItem() { Id = "123456" }; 
      string expectedLogMessage = "Registration record {id} was not retrieved.";
      _mockProvider
        .Setup(t => t.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          It.IsAny<bool>()))
        .ReturnsAsync((ErsReferralResult)null);

      // Act.
      bool result = await _classToTest.MockProcessNewReferral(
        workListEntry,
        false,
        false);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeFalse();
        AssertCreateLogVerify(workListEntry.Item.Id);
        _mockLogger.Verify(t => t.Debug(
          expectedLogMessage,
          workListEntry.Item.Id),
          Times.Once);
      }
    }

    [Fact]
    public async Task ErsWorkListEnty_IsNull_ReturnFalse_WithDebugLog()
    {
      // Arrange.
      string expectedLogInfoMessage =
        "{total} filename exclusions have been set.";
      string expectedLogMessage =
        "Registration record {id} was not retrieved.";
      _mockProvider
        .Setup(t => t.GetRegistration(
          null,
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          It.IsAny<bool>()))
        .ReturnsAsync((ErsReferralResult)null);

      // Act.
      bool result = await _classToTest.MockProcessNewReferral(
        null,
        false,
        false);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeFalse();      
        _mockLogger.Verify(t => t.Information(
          expectedLogInfoMessage,
          _config.Data.ExcludedFiles.Count),
          Times.Once);
        AssertCreateLogVerify();
        _mockLogger.Verify(t => t.Debug(
          expectedLogMessage,
          (string)null),
          Times.Once);
      }
    }

    [Fact]
    public async Task RegistrationRecorded_Success_false_InteropErrors()
    {
      // Arrange.
      string expectedWarningMessage = "There were errors with the Word " +
        "Interop while processing record {ubrn}. Skipping record creation.";
      ErsWorkListEntry workListEntry = new()
      {
        Item = new ERSWorkListItem() { Id = "123456" }
      };
      _ersReferralResult.InteropErrors = true;
      _ersReferralResult.WasRetrievedFromErs = true;
      _mockProvider
        .Setup(t => t.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          It.IsAny<bool>()))
        .ReturnsAsync(_ersReferralResult);

      // Act.
      bool result = await _classToTest.MockProcessNewReferral(
        workListEntry,
        false,
        false);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeFalse();
        AssertCreateLogVerify(workListEntry.Item.Id);
        _mockLogger.Verify(t => t.Warning(
          expectedWarningMessage,
          workListEntry.Item.Id),
          Times.Once);
      }
    }

    [Fact]
    public async Task
      RegistrationRecord_Success_NoValidAttachmentFound_LogDebug()
    {
      // Arrange.
      ErsWorkListEntry workListEntry = new()
      {
        Item = new ERSWorkListItem() { Id = "123456" }
      };
      _ersReferralResult.WasRetrievedFromErs = true;
      _ersReferralResult.NoValidAttachmentFound = true;

      _mockProvider
        .Setup(t => t.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          It.IsAny<bool>()))
        .ReturnsAsync(_ersReferralResult);

      _mockProvider
        .Setup(t => t.NewInvalidAttachment(
          It.IsAny<ReferralInvalidAttachmentPost>()))
        .Verifiable();

      // Act.
      bool result = await _classToTest.MockProcessNewReferral(
        workListEntry,
        false,
        false);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeTrue();
        AssertCreateLogVerify(workListEntry.Item.Id);
        _mockProvider.Verify(t => t.NewInvalidAttachment(
          It.IsAny<ReferralInvalidAttachmentPost>()),
          Times.Once);
      }
    }

    [Fact]
    public async Task
      RegistrationRecord_Success_ValidAttachmentFound_NhsNumberMismatch_LogDebug()
    {
      // Arrange.
      string expectedLogMessage = "NHS Number Mismatch for UBRN {ubrn}";

      Mock<ErsWorkListEntry> mockWorkListEntry = new();
      mockWorkListEntry
        .Setup(t => t.Item)
        .Returns(new ERSWorkListItem() { Id = _ubrn });
      mockWorkListEntry
        .Setup(t => t.NhsNumber)
        .Returns(Generators.GenerateNhsNumber(new Random()));

      _ersReferralResult.Success = true;
      _mockProvider
        .Setup(t => t.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          It.IsAny<bool>()))
        .ReturnsAsync(_ersReferralResult);

      _mockProvider
        .Setup(t => t.NewNhsNumberMismatch(
          It.IsAny<ReferralNhsNumberMismatchPost>()))
        .Verifiable();

      // Act.
      bool result = await _classToTest.MockProcessNewReferral(
        mockWorkListEntry.Object,
        false,
        false);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeTrue();
        AssertCreateLogVerify(mockWorkListEntry.Object.Item.Id);
        _mockLogger.Verify(t => t.Debug(
          expectedLogMessage,
          _ubrn),
          Times.Once);
        _mockProvider.Verify(t => t.NewNhsNumberMismatch(
          It.IsAny<ReferralNhsNumberMismatchPost>()),
          Times.Once);
      }
    }

    [Fact]
    public async Task DownloadCriDocument_Success_False_Log_Error()
    {
      // Arrange.
      string expectedLogMessage1 = "Clinical information not downloaded for " +
        "UBRN {ubrn}. See logs for details.";
      string expectedLogMessage2 = "There were test errors.";
      CreateReferralResult createReferralResult = new()
      {
        Success = false
      };
      createReferralResult.Errors.Add(_expectedTestErrorMessage);

      Mock<ErsWorkListEntry> mockWorkListEntry = new();
      mockWorkListEntry
        .Setup(t => t.Item)
        .Returns(new ERSWorkListItem() { Id = _ubrn });
      mockWorkListEntry
        .Setup(t => t.NhsNumber)
        .Returns(_nhsNumber);

      _ersReferralResult.Success = true;
      _mockProvider
        .Setup(t => t.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          It.IsAny<bool>()))
        .ReturnsAsync(_ersReferralResult);

      _mockProvider
        .Setup(t => t.GetCriDocument(
          It.IsAny<ErsSession>(),
          It.IsAny<string>(),
          It.IsAny<string>()))
        .ReturnsAsync(new GetCriResult() { Success = false });

      _mockProvider
        .Setup(t => t.CreateReferral(
          It.IsAny<ReferralPost>()))
        .ReturnsAsync(createReferralResult);


      // Act.
      bool result = await _classToTest.MockProcessNewReferral(
        mockWorkListEntry.Object,
        false,
        false);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeFalse();
        AssertCreateLogVerify(mockWorkListEntry.Object.Item.Id);
        _mockLogger.Verify(t => t.Error(
          expectedLogMessage1,
          _ubrn),
          Times.Once);
        _mockLogger.Verify(t => t.Error(expectedLogMessage2), Times.Once);
      }
    }

    [Fact]
    public async Task DownloadCriDocument_Success_NoCriDocument_Log()
    {
      // Arrange.
      string expectedLogMessage1 = "No Cri Document Available for UBRN {ubrn}.";
      string expectedLogMessage2 = "There were test errors.";
      CreateReferralResult createReferralResult = new()
      {
        Success = false
      };
      createReferralResult.Errors.Add(_expectedTestErrorMessage);

      Mock<ErsWorkListEntry> mockWorkListEntry = new();
      mockWorkListEntry
        .Setup(t => t.Item)
        .Returns(new ERSWorkListItem() { Id = _ubrn });
      mockWorkListEntry
        .Setup(t => t.NhsNumber)
        .Returns(_nhsNumber);

      _ersReferralResult.Success = true;
      _mockProvider
        .Setup(t => t.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          It.IsAny<bool>()))
        .ReturnsAsync(_ersReferralResult);

      _mockProvider
        .Setup(t => t.GetCriDocument(
          It.IsAny<ErsSession>(),
          It.IsAny<string>(),
          It.IsAny<string>()))
        .ReturnsAsync(new GetCriResult() {
          Success = true,
          NoCriDocumentFound = true });

      _mockProvider
        .Setup(t => t.CreateReferral(
          It.IsAny<ReferralPost>()))
        .ReturnsAsync(createReferralResult);


      // Act.
      bool result = await _classToTest.MockProcessNewReferral(
        mockWorkListEntry.Object,
        false,
        false);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeFalse();
        AssertCreateLogVerify(mockWorkListEntry.Object.Item.Id);
        _mockLogger.Verify(t => t.Information(
          expectedLogMessage1,
          _ubrn),
          Times.Once);
        _mockLogger.Verify(t => t.Error(expectedLogMessage2), Times.Once);
      }
    }

    [Fact]
    public async Task
      DownloadCriDocument_Success_AddDocumentToReferralPost_Log()
    {
      // Arrange.
      string expectedLogMessage1 = "There were test errors.";
      DateTimeOffset criLastUpdated = DateTimeOffset.Now;
      string criDocumentText = "This is a test document.";     
      byte[] criDocument = Encoding.UTF8.GetBytes(criDocumentText);
      string expectedCriDocumentText = Convert.ToBase64String(criDocument);
      CreateReferralResult createReferralResult = new()
      {
        Success = false
      };
      createReferralResult.Errors.Add(_expectedTestErrorMessage);

      Mock<ErsWorkListEntry> mockWorkListEntry = new();
      mockWorkListEntry
        .Setup(t => t.Item)
        .Returns(new ERSWorkListItem() { Id = _ubrn });
      mockWorkListEntry
        .Setup(t => t.NhsNumber)
        .Returns(_nhsNumber);
      mockWorkListEntry
        .Setup(t => t.ClinicalInfoLastUpdated)
        .Returns(criLastUpdated);

      _ersReferralResult.Success = true;
      _mockProvider
        .Setup(t => t.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          It.IsAny<bool>()))
        .ReturnsAsync(_ersReferralResult);

      _mockProvider
        .Setup(t => t.GetCriDocument(
          It.IsAny<ErsSession>(),
          It.IsAny<string>(),
          It.IsAny<string>()))
        .ReturnsAsync(new GetCriResult()
        {
          Success = true,
          NoCriDocumentFound = false,
          CriDocument = criDocument
        });

      _mockProvider
        .Setup(t => t.CreateReferral(
          It.IsAny<ReferralPost>()))
        .ReturnsAsync(createReferralResult);


      // Act.
      bool result = await _classToTest.MockProcessNewReferral(
        mockWorkListEntry.Object,
        false,
        false);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeFalse();
        AssertCreateLogVerify(mockWorkListEntry.Object.Item.Id);
        _mockLogger.Verify(t => t.Error(expectedLogMessage1), Times.Once);
        _referralPost.CriLastUpdated.Should().Be(criLastUpdated);
        _referralPost.CriDocument.Should()
          .BeEquivalentTo(expectedCriDocumentText);
      }
    }

    [Fact]
    public async Task CreateReferralResult_Success_CloseErsReferral_False_Log()
    {
      // Arrange.
      string expectedLogMessage1 = "CloseErsReferral is false for ERS " +
        "Referral {id}";
      DateTimeOffset criLastUpdated = DateTimeOffset.Now;
      string criDocumentText = "This is a test document.";
      byte[] criDocument = Encoding.UTF8.GetBytes(criDocumentText);
      string expectedCriDocumentText = Convert.ToBase64String(criDocument);
      CreateReferralResult createReferralResult = new()
      {
        Success = true,
        CloseErsReferral = false
      };

      Mock<ErsWorkListEntry> mockWorkListEntry = new();
      mockWorkListEntry
        .Setup(t => t.Item)
        .Returns(new ERSWorkListItem() { Id = _ubrn });
      mockWorkListEntry
        .Setup(t => t.NhsNumber)
        .Returns(_nhsNumber);
      mockWorkListEntry
        .Setup(t => t.ClinicalInfoLastUpdated)
        .Returns(criLastUpdated);

      _ersReferralResult.Success = true;
      _mockProvider
        .Setup(t => t.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          It.IsAny<bool>()))
        .ReturnsAsync(_ersReferralResult);

      _mockProvider
        .Setup(t => t.GetCriDocument(
          It.IsAny<ErsSession>(),
          It.IsAny<string>(),
          It.IsAny<string>()))
        .ReturnsAsync(new GetCriResult()
        {
          Success = true,
          NoCriDocumentFound = false,
          CriDocument = criDocument
        });

      _mockProvider
        .Setup(t => t.CreateReferral(
          It.IsAny<ReferralPost>()))
        .ReturnsAsync(createReferralResult);

      // Act.
      bool result = await _classToTest.MockProcessNewReferral(
        mockWorkListEntry.Object,
        false,
        false);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeTrue();
        AssertCreateLogVerify(mockWorkListEntry.Object.Item.Id);
        _mockLogger.Verify(t => t.Debug(
          expectedLogMessage1,
          _ubrn),
          Times.Once);
        _referralPost.CriLastUpdated.Should().Be(criLastUpdated);
        _referralPost.CriDocument.Should()
          .BeEquivalentTo(expectedCriDocumentText);
      }
    }

    [Fact]
    public async Task
      CreateReferralResult_Success_CloseErsReferral_RecordOutcome_False()
    {
      // Arrange.
      string expectedLogMessage1 = "Closing ERS Referral {id}";
      DateTimeOffset criLastUpdated = DateTimeOffset.Now;
      string criDocumentText = "This is a test document.";
      byte[] criDocument = Encoding.UTF8.GetBytes(criDocumentText);
      string expectedCriDocumentText = Convert.ToBase64String(criDocument);
      CreateReferralResult createReferralResult = new()
      {
        Success = true,
        CloseErsReferral = true
      };
      ReviewCommentResult reviewCommentResult = new()
      {
        Success = false
      };
      reviewCommentResult.Errors.Add(_expectedTestErrorMessage);

      Mock<ErsWorkListEntry> mockWorkListEntry = new();
      mockWorkListEntry
        .Setup(t => t.Item)
        .Returns(new ERSWorkListItem() { Id = _ubrn });
      mockWorkListEntry
        .Setup(t => t.NhsNumber)
        .Returns(_nhsNumber);
      mockWorkListEntry
        .Setup(t => t.ClinicalInfoLastUpdated)
        .Returns(criLastUpdated);

      _ersReferralResult.Success = true;
      _mockProvider
        .Setup(t => t.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          It.IsAny<bool>()))
        .ReturnsAsync(_ersReferralResult);

      _mockProvider
        .Setup(t => t.GetCriDocument(
          It.IsAny<ErsSession>(),
          It.IsAny<string>(),
          It.IsAny<string>()))
        .ReturnsAsync(new GetCriResult()
        {
          Success = true,
          NoCriDocumentFound = false,
          CriDocument = criDocument
        });

      _mockProvider
        .Setup(t => t.GetAvailableActions(
          It.IsAny<IErsSession>(),
          It.IsAny<IErsReferral>(),
          It.IsAny<string>()))
        .ReturnsAsync(_availableActionResult);

      _mockProvider
        .Setup(t => t.RecordOutcome(
          It.IsAny<string>(),
          It.IsAny<IErsReferral>(),
          It.IsAny<string>(),
          It.IsAny<Enums.Outcome>(),
          It.IsAny<ErsSession>()))
        .ReturnsAsync(reviewCommentResult);

      _mockProvider
        .Setup(t => t.CreateReferral(
          It.IsAny<ReferralPost>()))
        .ReturnsAsync(createReferralResult);

      // Act.
      bool result = await _classToTest.MockProcessNewReferral(
        mockWorkListEntry.Object,
        false,
        false);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeTrue();
        AssertCreateLogVerify(mockWorkListEntry.Object.Item.Id);
        _mockLogger.Verify(t => t.Debug(
          expectedLogMessage1,
          _ubrn),
          Times.Once);
        _mockLogger.Verify(t => t.Error(
          _expectedTestErrorMessage),
          Times.Once);
        _referralPost.CriLastUpdated.Should().Be(criLastUpdated);
        _referralPost.CriDocument.Should()
          .BeEquivalentTo(expectedCriDocumentText);
      }
    }

    [Fact]
    public async Task SetErsReferralClosed_RecordOutcome()
    {
      // Arrange.
      string expectedLogMessage1 = "Closing ERS Referral {id}";
      DateTimeOffset criLastUpdated = DateTimeOffset.Now;
      string criDocumentText = "This is a test document.";
      byte[] criDocument = Encoding.UTF8.GetBytes(criDocumentText);
      string expectedCriDocumentText = Convert.ToBase64String(criDocument);
      CreateReferralResult createReferralResult = new()
      {
        Success = true,
        CloseErsReferral = true,
        ReferralId = Guid.NewGuid()
      };
      ReviewCommentResult reviewCommentResult = new()
      {
        Success = true
      };

      Mock<ErsWorkListEntry> mockWorkListEntry = new();
      mockWorkListEntry
        .Setup(t => t.Item)
        .Returns(new ERSWorkListItem() { Id = _ubrn });
      mockWorkListEntry
        .Setup(t => t.NhsNumber)
        .Returns(_nhsNumber);
      mockWorkListEntry
        .Setup(t => t.ClinicalInfoLastUpdated)
        .Returns(criLastUpdated);

      _ersReferralResult.Success = true;
      _mockProvider
        .Setup(t => t.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          It.IsAny<bool>()))
        .ReturnsAsync(_ersReferralResult);

      _mockProvider
        .Setup(t => t.GetCriDocument(
          It.IsAny<ErsSession>(),
          It.IsAny<string>(),
          It.IsAny<string>()))
        .ReturnsAsync(new GetCriResult()
        {
          Success = true,
          NoCriDocumentFound = false,
          CriDocument = criDocument
        });

      _mockProvider
        .Setup(t => t.GetAvailableActions(
          It.IsAny<IErsSession>(),
          It.IsAny<IErsReferral>(),
          It.IsAny<string>()))
        .ReturnsAsync(_availableActionResult);

      _mockProvider
        .Setup(t => t.RecordOutcome(
          It.IsAny<string>(),
          It.IsAny<IErsReferral>(),
          It.IsAny<string>(),
          It.IsAny<Enums.Outcome>(),
          It.IsAny<ErsSession>()))
        .ReturnsAsync(reviewCommentResult);

      _mockProvider
        .Setup(t => t.SetErsReferralClosed(
          It.IsAny<string>()))
        .Verifiable();

      _mockProvider
        .Setup(t => t.CreateReferral(
          It.IsAny<ReferralPost>()))
        .ReturnsAsync(createReferralResult);

      // Act.
      bool result = await _classToTest.MockProcessNewReferral(
        mockWorkListEntry.Object,
        false,
        false);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().BeTrue();
        AssertCreateLogVerify(mockWorkListEntry.Object.Item.Id);
        _mockLogger.Verify(t => t.Debug(
          expectedLogMessage1,
          _ubrn),
          Times.Once);
        _referralPost.CriLastUpdated.Should().Be(criLastUpdated);
        _referralPost.CriDocument.Should()
          .BeEquivalentTo(expectedCriDocumentText);
        _mockProvider.Verify(t => t.SetErsReferralClosed(
          It.IsAny<string>()),
          Times.Once);
      }
    }

    private void AssertCreateLogVerify(string id = null)
    {
      string expectedDebugLog = "Creating item {id}";
      _mockLogger.Verify(t => t.ForContext<ReferralProcessor>(), Times.Once);
      _mockLogger.Verify(t => t.Debug(
          expectedDebugLog,
          (string)id),
          Times.Once);
    }

  }
}


