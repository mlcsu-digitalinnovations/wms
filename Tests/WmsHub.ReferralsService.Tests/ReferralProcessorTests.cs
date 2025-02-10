using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Serilog;
using WmsHub.Common.Api.Models;
using WmsHub.ReferralsService.Interfaces;
using WmsHub.ReferralsService.Models;
using WmsHub.ReferralsService.Models.Configuration;
using WmsHub.ReferralsService.Models.Results;
using WmsHub.Common.Models;
using WmsHub.ReferralsService.Pdf;
using WmsHub.Common.Enums;

namespace WmsHub.ReferralsService.Tests;

public class ReferralProcessorTests
{
  private ReferralProcessor _classToTest;

  private readonly Mock<IReferralsDataProvider> _mockProvider =
    new Mock<IReferralsDataProvider>();

  private readonly Mock<ISmartCardAuthentictor> _mockSmartCardAuthenticator =
    new Mock<ISmartCardAuthentictor>();

  private readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();
  private ErsSession _session;
  private Config _config;

  public ReferralProcessorTests()
  {

    _config = new Config
    {
      Data = new DataConfiguration()
      {
        ExcludedFiles = new() { "obs" }
      }
    };

    _mockLogger.Setup(t => t.Information(It.IsAny<string>())).Verifiable();

  }

  public class ProcessTests : ReferralProcessorTests
  {
    private Mock<WorkListResult> _mockWorkListResult = new();
    private Mock<ErsWorkList> _mockErsWorkList = new();
    private Mock<ErsWorkListEntry> _mockErsWorkListEntry = new();

    private Mock<ERSWorkListItem> _mockERSWorkListItem = new();

    private RegistrationListResult _registrationListResult;

    public ProcessTests()
    {
      _mockERSWorkListItem.Object.Id = "123";
      _mockERSWorkListItem.Object.Reference = "type/123456789123";
      _mockERSWorkListItem.Object.ResourceType = "Type";

      _mockErsWorkListEntry.Setup(t => t.Item)
        .Returns(_mockERSWorkListItem.Object);
      _mockErsWorkListEntry.Setup(t => t.NhsNumber).Returns("123456789123");

      _mockErsWorkList.Object.Entry = new[]
      {
        _mockErsWorkListEntry.Object
      };
      _mockWorkListResult.Setup(t => t.Success).Returns(true);

      _mockWorkListResult.Object.WorkList = _mockErsWorkList.Object;


    }

    [Fact] //TODO: #1620 Bug
    public async Task ProcessReportOnlyTest()
    {
      // Arrange.
      _registrationListResult = new RegistrationListResult
      {
        Success = true
      };
      _registrationListResult.ReferralUbrnList = new RegistrationList
      {
        Ubrns = new List<GetActiveUbrnResponse>
        {
          new GetActiveUbrnResponse
          {
            Status = "CancelledByEreferralsStaging",
            Ubrn = "123456789123",
            ReferralAttachmentId = "100"
          }
        }
      };
      _session = new ErsSession()
      {
        Id = "123",
        SmartCardToken = "TestToken"
      };
      _mockSmartCardAuthenticator.Setup(t => t.CreateSession())
        .Returns(Task.FromResult(true));
      _mockSmartCardAuthenticator.Setup(t => t.ActiveSession)
        .Returns(_session);
      _mockProvider.Setup(t => t.GetWorkListFromErs(_session))
        .Returns(Task.FromResult(_mockWorkListResult.Object));
      _mockProvider.Setup((t => t.GetReferralList(true)))
        .Returns(Task.FromResult(_registrationListResult));
      _classToTest = new ReferralProcessor(_mockProvider.Object,
        _mockSmartCardAuthenticator.Object, _config, _mockLogger.Object,
        _mockLogger.Object);
      // Act.
      var result = await _classToTest.Process(true);
      // Assert.
      // TODO This test needs an implementation
    }

    [Fact] //TODO: #1620 Bug
    public async Task ProcessReportOnly_NewRecord_NoAttachmentId()
    {
      // Arrange.
      ErsReferralResult ersReferralResult = new ErsReferralResult()
      {
        Success = true,
        NoValidAttachmentFound = true
      };
      _registrationListResult = new RegistrationListResult
      {
        Success = true
      };
      _registrationListResult.ReferralUbrnList = new RegistrationList
      {
        Ubrns = new List<GetActiveUbrnResponse>
        {
          new GetActiveUbrnResponse
          {
            Status = "New",
            Ubrn = "123456789124"
          }
        }
      };
      _session = new ErsSession()
      {
        Id = "123",
        SmartCardToken = "TestToken"
      };
      _mockSmartCardAuthenticator.Setup(t => t.CreateSession())
        .Returns(Task.FromResult(true));
      _mockSmartCardAuthenticator.Setup(t => t.ActiveSession)
        .Returns(_session);
      _mockProvider.Setup(t => t.GetWorkListFromErs(_session))
        .Returns(Task.FromResult(_mockWorkListResult.Object));
      _mockProvider.Setup((t => t.GetReferralList(false)))
        .Returns(Task.FromResult(_registrationListResult));
      _mockProvider.Setup(t => t.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          false))
        .ReturnsAsync(ersReferralResult);
      _mockProvider.Setup(t =>
          t.UpdateMissingAttachment(It.IsAny<ReferralMissingAttachmentPost>()))
        .Verifiable();
      _classToTest = new ReferralProcessor(_mockProvider.Object,
        _mockSmartCardAuthenticator.Object, _config, _mockLogger.Object,
        _mockLogger.Object);
      // Act.
      ProcessExecutionResult result = await _classToTest.Process(false);
      // Assert.
      result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Process_NewRecord_NhsNumberMissmatch()
    {
      // Arrange.
      Mock<ReferralPost> mockReferralPost = new();
      mockReferralPost.Object.NhsNumber = "9999439993";

      Mock<ReferralAttachmentPdfProcessor> mockPdf =
        new(_mockLogger.Object, _mockLogger.Object, null, null, null);

      mockPdf.Setup(x => x
        .GenerateReferralCreationObject(
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>()))
        .Returns(mockReferralPost.Object);

      Mock<ErsReferralResult> mockErsReferralResult = new();
      mockErsReferralResult.Setup(x => x.Success).Returns(true);
      mockErsReferralResult.Object.NoValidAttachmentFound = false;
      mockErsReferralResult.Object.AttachmentId = "123";
      mockErsReferralResult.Object.Pdf = mockPdf.Object;
      mockErsReferralResult.Object.CriDocumentDate = DateTime.Now;

      _registrationListResult = new RegistrationListResult
      {
        Success = true
      };

      _registrationListResult.ReferralUbrnList = new RegistrationList
      {
        Ubrns = new List<GetActiveUbrnResponse>
        {
          new GetActiveUbrnResponse
          {
            Status = "New",
            Ubrn = "123456789124",
            CriLastUpdated = DateTime.Now
          }
        }
      };

      _session = new ErsSession()
      {
        Id = "123",
        SmartCardToken = "TestToken"
      };

      _mockSmartCardAuthenticator.Setup(x => x.CreateSession())
        .Returns(Task.FromResult(true));

      _mockSmartCardAuthenticator.Setup(x => x.ActiveSession)
        .Returns(_session);

      _mockProvider.Setup(x => x.GetWorkListFromErs(_session))
        .Returns(Task.FromResult(_mockWorkListResult.Object));

      _mockProvider.Setup((x => x.GetReferralList(false)))
        .Returns(Task.FromResult(_registrationListResult));

      _mockProvider.Setup(x => x.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          false))
        .ReturnsAsync(mockErsReferralResult.Object);

      _mockProvider.Setup(x => x
        .NewNhsNumberMismatch(It.IsAny<ReferralNhsNumberMismatchPost>()))
        .Verifiable();

      _classToTest = new(
        _mockProvider.Object,
        _mockSmartCardAuthenticator.Object,
        _config,
        _mockLogger.Object,
        _mockLogger.Object);

      // Act.
      ProcessExecutionResult result = await _classToTest.Process(false);

      // Assert.
      _mockProvider.Verify();
      result.Completed.Should().BeTrue();
    }

    [Fact]
    public async Task Process_ExistingRecord_NhsNumberMissmatch()
    {
      // Arrange.
      _mockERSWorkListItem.Object.Reference = "type/123456789124";
      Mock<ReferralPut> mockReferralPut = new Mock<ReferralPut>();
      mockReferralPut.Object.NhsNumber = "9999439993";

      Mock<ReferralAttachmentPdfProcessor> mockPdf =
        new(_mockLogger.Object, _mockLogger.Object, null, null, null);

      mockPdf
        .Setup(x => x.GenerateReferralUpdateObject(
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>()))
        .Returns(mockReferralPut.Object);

      Mock<ErsReferralResult> mockErsReferralResult = new();
      mockErsReferralResult.Setup(x => x.Success).Returns(true);
      mockErsReferralResult.Object.NoValidAttachmentFound = false;
      mockErsReferralResult.Object.AttachmentId = "123";
      mockErsReferralResult.Object.MostRecentAttachmentDate = DateTimeOffset.Now.AddDays(-1);
      mockErsReferralResult.Object.Pdf = mockPdf.Object;
      mockErsReferralResult.Object.CriDocumentDate = DateTime.Now;

      _registrationListResult = new RegistrationListResult
      {
        Success = true
      };

      _registrationListResult.ReferralUbrnList = new RegistrationList
      {
        Ubrns = new List<GetActiveUbrnResponse>
        {
          new GetActiveUbrnResponse
          {
            Status = ErsReferralStatus.AwaitingUpdate.ToString(),
            Ubrn = "123456789124",
            ReferralAttachmentId = "124",
            MostRecentAttachmentDate = DateTimeOffset.Now,
            CriLastUpdated = DateTime.Now
          }
        }
      };

      _session = new ErsSession()
      {
        Id = "124",
        SmartCardToken = "TestToken"
      };

      _mockSmartCardAuthenticator.Setup(x => x.CreateSession())
        .Returns(Task.FromResult(true));

      _mockSmartCardAuthenticator.Setup(x => x.ActiveSession)
        .Returns(_session);

      _mockProvider.Setup(x => x.GetWorkListFromErs(_session))
        .Returns(Task.FromResult(_mockWorkListResult.Object));

      _mockProvider.Setup(x => x.GetReferralList(false))
        .Returns(Task.FromResult(_registrationListResult));

      _mockProvider.Setup(x => x.GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          false))
        .ReturnsAsync(mockErsReferralResult.Object);

      _mockProvider.Setup(x => x.UpdateNhsNumberMismatch(It.IsAny<ReferralNhsNumberMismatchPost>()))
        .Verifiable();

      _classToTest = new ReferralProcessor(
        _mockProvider.Object,
        _mockSmartCardAuthenticator.Object,
        _config,
        _mockLogger.Object,
        _mockLogger.Object);

      // Act.
      ProcessExecutionResult result = await _classToTest.Process(false);

      // Assert.
      _mockProvider.Verify();
      result.Completed.Should().BeTrue();
      result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Process_NewRecord__InvalidAttachment_InvalidNhsNumber()
    {
      // Arrange.
      Mock<ReferralPost> mockReferralPost = new();
      mockReferralPost.Object.NhsNumber = "12345";

      Mock<ReferralAttachmentPdfProcessor> mockPdf = new
        (_mockLogger.Object, _mockLogger.Object, null, null, null);

      mockPdf
        .Setup(x => x.GenerateReferralCreationObject(
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>()))
        .Returns(mockReferralPost.Object);

      Mock<ErsReferralResult> mockErsReferralResult = new();
      mockErsReferralResult.Setup(x => x.Success).Returns(true);
      mockErsReferralResult.Object.MostRecentAttachmentDate = DateTimeOffset.Now;
      mockErsReferralResult.Object.NoValidAttachmentFound = false;
      mockErsReferralResult.Object.AttachmentId = "123";
      mockErsReferralResult.Object.Pdf = mockPdf.Object;
      mockErsReferralResult.Object.CriDocumentDate = DateTime.Now;

      _registrationListResult = new RegistrationListResult
      {
        Success = true
      };

      _registrationListResult.ReferralUbrnList = new RegistrationList
      {
        Ubrns = new List<GetActiveUbrnResponse>
        {
          new GetActiveUbrnResponse
          {
            Status = "New",
            Ubrn = "123456789124",
            CriLastUpdated = DateTime.Now
          }
        }
      };

      _session = new ErsSession()
      {
        Id = "123",
        SmartCardToken = "TestToken"
      };

      _mockSmartCardAuthenticator.Setup(x => x.CreateSession())
        .Returns(Task.FromResult(true));

      _mockSmartCardAuthenticator.Setup(x => x.ActiveSession)
        .Returns(_session);

      _mockProvider.Setup(x => x.GetWorkListFromErs(_session))
        .Returns(Task.FromResult(_mockWorkListResult.Object));

      _mockProvider.Setup(x => x.GetReferralList(false))
        .Returns(Task.FromResult(_registrationListResult));

      _mockProvider.Setup(x => x
        .GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          false))
        .ReturnsAsync(mockErsReferralResult.Object);

      _mockProvider
        .Setup(x => x.NewInvalidAttachment(It.IsAny<ReferralInvalidAttachmentPost>()))
        .Verifiable();

      _classToTest = new ReferralProcessor(
        _mockProvider.Object,
        _mockSmartCardAuthenticator.Object,
        _config,
        _mockLogger.Object,
        _mockLogger.Object);

      // Act.
      ProcessExecutionResult result = await _classToTest.Process(false);

      // Assert.
      _mockProvider.Verify();
      result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Process_ExistingRecord_InvalidAttachment_InvalidNhsNumber()
    {
      // Arrange.
      _mockERSWorkListItem.Object.Reference = "type/123456789124";
      Mock<ReferralPut> mockReferralPut = new();
      mockReferralPut.Object.NhsNumber = "12345";

      Mock<ReferralAttachmentPdfProcessor> mockPdf =
        new(_mockLogger.Object, _mockLogger.Object, null, null, null);

      mockPdf.Setup(x => x
        .GenerateReferralUpdateObject(
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>()))
        .Returns(mockReferralPut.Object);

      Mock<ErsReferralResult> mockErsReferralResult = new();
      mockErsReferralResult.Setup(x => x.Success).Returns(true);
      mockErsReferralResult.Object.NoValidAttachmentFound = false;
      mockErsReferralResult.Object.AttachmentId = "123";
      mockErsReferralResult.Object.MostRecentAttachmentDate = DateTimeOffset.Now.AddDays(-1);
      mockErsReferralResult.Object.Pdf = mockPdf.Object;
      mockErsReferralResult.Object.CriDocumentDate = DateTime.Now;

      _registrationListResult = new RegistrationListResult
      {
        Success = true
      };

      _registrationListResult.ReferralUbrnList = new RegistrationList
      {
        Ubrns = new List<GetActiveUbrnResponse>
        {
          new GetActiveUbrnResponse
          {
            Status = ErsReferralStatus.AwaitingUpdate.ToString(),
            Ubrn = "123456789124",
            ReferralAttachmentId = "124",
            MostRecentAttachmentDate = DateTimeOffset.Now,
            CriLastUpdated = DateTime.Now
          }
        }
      };

      _session = new ErsSession()
      {
        Id = "124",
        SmartCardToken = "TestToken"
      };

      _mockSmartCardAuthenticator.Setup(x => x.CreateSession())
        .Returns(Task.FromResult(true));

      _mockSmartCardAuthenticator.Setup(x => x.ActiveSession)
        .Returns(_session);

      _mockProvider.Setup(x => x.GetWorkListFromErs(_session))
        .Returns(Task.FromResult(_mockWorkListResult.Object));

      _mockProvider.Setup((x => x.GetReferralList(false)))
        .Returns(Task.FromResult(_registrationListResult));

      _mockProvider.Setup(x => x
        .GetRegistration(
          It.IsAny<ErsWorkListEntry>(),
          It.IsAny<string>(),
          It.IsAny<string>(),
          It.IsAny<DateTimeOffset?>(),
          It.IsAny<ErsSession>(),
          false))
        .ReturnsAsync(mockErsReferralResult.Object);

      _mockProvider.Setup(x => x
        .UpdateInvalidAttachment(It.IsAny<ReferralInvalidAttachmentPost>()))
        .Verifiable();

      _classToTest = new ReferralProcessor(
        _mockProvider.Object,
        _mockSmartCardAuthenticator.Object,
        _config,
        _mockLogger.Object,
        _mockLogger.Object);

      // Act.
      ProcessExecutionResult result = await _classToTest.Process(false);

      // Assert.
      _mockProvider.Verify();
      result.Completed.Should().BeTrue();
      result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Process_ErsWorklistPrioritisation_WithEntries()
    {
      ErsWorkList testWorklist = new ErsWorkList()
      {
        Entry = new ErsWorkListEntry[]
        {
          new ErsWorkListEntry()
          {
            Item = new ERSWorkListItem() { Id = "Existing01" }
          },
          new ErsWorkListEntry()
          {
            Item = new ERSWorkListItem() { Id = "New01" }
          },
          new ErsWorkListEntry()
          {
            Item = new ERSWorkListItem() { Id = "Existing02" }
          },
          new ErsWorkListEntry()
          {
            Item = new ERSWorkListItem() { Id = "New02" }
          },
          new ErsWorkListEntry()
          {
            Item = new ERSWorkListItem() { Id = "Existing03" }
          }
        }
      };

      RegistrationList testRegistrationList = new()
      {
        Ubrns = new List<GetActiveUbrnResponse>()
          {
            new GetActiveUbrnResponse() { Ubrn = "Existing01"},
            new GetActiveUbrnResponse() { Ubrn = "Existing02"},
            new GetActiveUbrnResponse() { Ubrn = "Existing03"},
            new GetActiveUbrnResponse() { Ubrn = "Existing04"},
            new GetActiveUbrnResponse() { Ubrn = "Existing05"},
          }
      };

      _mockSmartCardAuthenticator.Setup(t => t.CreateSession())
       .Returns(Task.FromResult(true));
      _mockSmartCardAuthenticator.Setup(t => t.ActiveSession)
       .Returns(_session);
      _mockProvider.Setup(t => t.GetWorkListFromErs(_session))
       .Returns(Task.FromResult(_mockWorkListResult.Object));
      _mockProvider.Setup((t => t.GetReferralList(true)))
       .Returns(Task.FromResult(_registrationListResult));
      _classToTest = new ReferralProcessor(_mockProvider.Object,
        _mockSmartCardAuthenticator.Object, _config, _mockLogger.Object,
        _mockLogger.Object);

      _classToTest.PrioritiseWorklistItems(testWorklist, testRegistrationList);

      bool result = (testWorklist.Entry.Length == 5);
      result.Should().BeTrue();
      result = (testWorklist.Entry[0].Ubrn == "New01");
      result.Should().BeTrue();
      result = (testWorklist.Entry[1].Ubrn == "New02");
      result.Should().BeTrue();
      result = (testWorklist.Entry[2].Ubrn == "Existing01");
      result.Should().BeTrue();
      result = (testWorklist.Entry[3].Ubrn == "Existing02");
      result.Should().BeTrue();
      result = (testWorklist.Entry[4].Ubrn == "Existing03");
      result.Should().BeTrue();

      await Task.CompletedTask;
    }
  }

  public class ReferralService_ProcessorTests : ReferralProcessor
  {
    private static readonly Mock<ILogger> _mockLogger = new Mock<ILogger>();

    private static readonly Mock<ISmartCardAuthentictor>
      _mockSmartCardAuthenticator = new Mock<ISmartCardAuthentictor>();

    private static readonly Mock<IReferralsDataProvider> _mockDataProvider =
      new Mock<IReferralsDataProvider>();

    private ErsSession _session;

    private static readonly Config s_config = new()
    {
      Data = new DataConfiguration()
      {
        ExcludedFiles = ["obs"]
      }
    };

    public ReferralService_ProcessorTests() : base(
      _mockDataProvider.Object,
      _mockSmartCardAuthenticator.Object,
      s_config,
      _mockLogger.Object, 
      _mockLogger.Object)
    { }

    public class ProcessTest : ReferralService_ProcessorTests
    {
      private string _ubrn = "123456789123";

      private readonly Mock<WorkListResult> _workListResult =
        new Mock<WorkListResult>();

      private readonly Mock<RegistrationListResult> _registrationList =
        new Mock<RegistrationListResult>();

      private readonly Mock<RegistrationList> _referralUbrnList =
        new Mock<RegistrationList>();

      public ProcessTest()
      {
        _mockSmartCardAuthenticator.Setup(t => t.CreateSession())
          .Returns(Task.FromResult(true));
        _session = new ErsSession()
        {
          Id = "123",
          SmartCardToken = "TestToken"
        };
        _mockSmartCardAuthenticator.Setup(t => t.ActiveSession)
          .Returns(_session);

        var item = new ERSWorkListItem()
        {
          Id = _ubrn
        };

        var entry = new Common.Models.ErsWorkListEntry()
        {
          Item = item
        };


        ErsWorkList workList = new ErsWorkList()
        {
          Entry = new[] { entry }
        };

        GetActiveUbrnResponse activeUbrnResponse = new GetActiveUbrnResponse()
        {
          Ubrn = _ubrn
        };

        _workListResult.Setup(t => t.Success).Returns(true);
        _workListResult.Setup(t => t.WorkList).Returns(workList);

        _referralUbrnList.Setup(t => t.FindByUbrn(It.IsAny<string>()))
          .Returns(activeUbrnResponse);

        _registrationList.Setup(t => t.Success).Returns(true);
        _registrationList.Setup(t => t.ReferralUbrnList)
          .Returns(_referralUbrnList.Object);

        _mockDataProvider
          .Setup(t => t.GetWorkListFromErs(It.IsAny<ErsSession>()))
          .ReturnsAsync(_workListResult.Object);
        _mockDataProvider.Setup(t => t.GetReferralList(true))
          .ReturnsAsync(_registrationList.Object);
      }


      protected override async Task<int> ProcessRejectReferralAsync(
        List<GetActiveUbrnResponse> registeredUbrns,
        ErsWorkListEntry[] ersEntries,
        bool reportOnly)
      {
        return await Task.FromResult(1);
      }

      protected override async Task<bool> ProcessNewReferral(
        ErsWorkListEntry workListEntry,
        bool reportOnly,
        bool showDiagnostics,
        string attachmentId = null)
      {
        return await Task.FromResult(false);
      }

      public override async Task<bool> ProcessExistingReferral(
        GetActiveUbrnResponse wmsRecord,
        ErsWorkListEntry ersRecord,
        bool reportOnly,
        bool showDiagnostics,
        string overrideAttachmentId,
        bool reprocessUnchangedAttachment)
      {

        if (wmsRecord.ReferralAttachmentId == "101")
        {
          Assert.True(true, "wmsRecord == null");
          throw new ArgumentNullException("Active Ubrn Record should not be null");
        }

        if (wmsRecord.ReferralAttachmentId == "102")
        {
          Assert.True(true, "ersRecord == null");
          throw new ArgumentNullException("Ers Worklist Entry should not be null");
        }

        if (wmsRecord.ReferralAttachmentId == "103")
        {
          Assert.True(true, "DataProvider Errors");
          return await Task.FromResult(false);
        }

        if (wmsRecord.ReferralAttachmentId == "104")
        {
          Assert.True(true, "registrationRecord.AttachmentId == wmsRecord.ReferralAttachmentId");
          return await Task.FromResult(true);
        }

        if (reportOnly)
        {
          return await Task.FromResult(true);
        }

        if (wmsRecord.ReferralAttachmentId == "105")
        {
          Assert.True(true, "updateReferralResult.Success == false");
          return await Task.FromResult(false);
        }

        return await Task.FromResult(true);
      }
    }

    public class ProcessBatchTests : ReferralService_ProcessorTests
    {
      private readonly string _error =
        "Value cannot be null. (Parameter '{0}')";

      private readonly Mock<ILogger> _moqLogger = new Mock<ILogger>();

      private readonly Mock<ReferralAttachmentPdfProcessor>
        _mockPdfProcessor;

      public ProcessBatchTests()
      {
        _mockPdfProcessor = new(_mockLogger.Object);
      }

      [Fact]
      public async Task BatchIsNull_ArgumentNullException_Expected()
      {
        // Arrange.
        string expected = string.Format(_error, "Batch was null.");
        // Act.
        try
        {
          ProcessExecutionResult response = await Process(batch: null, false);
          // Assert.
          throw new Exception("Expected exception not returned");
        }
        catch (ArgumentNullException ex)
        {
          Assert.True(true, ex.Message);
          ex.Message.Should().Be(expected);
        }
        catch (Exception ex)
        {
          Assert.Fail(ex.Message);

        }
      }

      [Fact]
      public async Task BatchItems_Null_ArgumentNullException_Expected()
      {
        // Arrange.
        string expected = string.Format(_error, "Batch contained no items.");
        Batch batch = new Batch();
        // Act.
        try
        {
          ProcessExecutionResult response = await Process(batch: batch, false);
          // Assert.
          throw new Exception("Expected exception not returned");
        }
        catch (ArgumentNullException ex)
        {
          Assert.True(true, ex.Message);
          ex.Message.Should().Be(expected);
        }
        catch (Exception ex)
        {
          Assert.Fail(ex.Message);
        }
      }

      [Fact]
      public async Task BatchItems_0_InvalidDataException_Expected()
      {
        // Arrange.
        string expected = "Batch was empty.";
        Batch batch = new Batch();
        batch.Items = new List<BatchItem>();
        // Act.
        try
        {
          ProcessExecutionResult response = await Process(batch: batch, false);
          // Assert.
          throw new Exception("Expected exception not returned");
        }
        catch (InvalidDataException ex)
        {
          Assert.True(true, ex.Message);
        }
        catch (Exception ex)
        {
          Assert.Fail(ex.Message);
          ex.Message.Should().Be(expected);
        }
      }
    }
  }

  //public class RecordReviewActionsTests : ReferralProcessor, IDisposable
  //{
  //  private static readonly Mock<AvailableActions> _availableActionsMock
  //    = new();
  //  private static readonly Mock<IReviewCommentResult> _commentResultMock
  //    = new();
  //  private static readonly Mock<IReferralsDataProvider> _dataProviderMock
  //    = new();
  //  private static readonly Mock<IGetDischargeUbrnResponse> _dischargeMock
  //    = new();
  //  private static readonly Mock<IGetDischargeListResult> _dischargeResultMock
  //    = new();
  //  private static readonly Mock<IErsReferral> _ersReferralMock = new();
  //  private static readonly Mock<ILogger> _loggerMock = new();
  //  private ErsSession _session;
  //  private static readonly Mock<ISmartCardAuthentictor>
  //    _smartCardAuthenticatorMock = new();

  //  public RecordReviewActionsTests() :
  //    base(_dataProviderMock.Object,
  //      _smartCardAuthenticatorMock.Object,
  //      null,
  //      _loggerMock.Object, _loggerMock.Object)
  //  {
  //    _commentResultMock.SetupProperty(c => c.Success);
  //  }

  //  public void Dispose()
  //  {
  //    _loggerMock.Reset();
  //  }

  //  [Fact]
  //  public async Task WhenErsDischargeIsNull_ShouldWriteErrorLogAndSetDischargeResultSuccessToFalse()
  //  {
  //    // Arrange.
  //    string ubrnToTest = Generators.GenerateUbrnGp(new Random());
  //    string expectedDebugMessage = "(IGetDischargeUbrnResponse)discharge" +
  //      " cannot be null here.";
  //    bool reportMode = false;

  //    _dischargeMock
  //      .Setup(t => t.Ubrn)
  //      .Returns(ubrnToTest);

  //    // Act.
  //    IReviewCommentResult result = await RecordReviewOutcome(
  //      _commentResultMock.Object,
  //      null,
  //      _dischargeResultMock.Object,
  //      null,
  //      reportMode);

  //    // Assert.
  //    _loggerMock.Verify(t =>
  //      t.Error(expectedDebugMessage),
  //      Times.Once);

  //    _dischargeResultMock.Object.Success.Should().BeFalse();
  //  }

  //  [Fact]
  //  public async Task WhenErsReferralIsNull_ShouldWriteErrorLogAndSetDischargeResultSuccessToFalse()
  //  {
  //    // Arrange.
  //    string ubrnToTest = Generators.GenerateUbrnGp(new Random());
  //    string expectedDebugMessage = "Failed to retrieve UBRN {ubrn} to " +
  //      "record outcome.";
  //    bool reportMode = false;

  //    _dischargeMock
  //      .Setup(t => t.Ubrn)
  //      .Returns(ubrnToTest);

  //    // Act.
  //    IReviewCommentResult result = await RecordReviewOutcome(
  //      _commentResultMock.Object,
  //      _dischargeMock.Object,
  //      _dischargeResultMock.Object,
  //      null,
  //      reportMode);

  //    // Assert.
  //    _loggerMock.Verify(t =>
  //      t.Error(expectedDebugMessage, ubrnToTest),
  //      Times.Once);

  //    _dischargeResultMock.Object.Success.Should().BeFalse();
  //  }

  //  [Fact]
  //  public async Task WhenAvailableActionSuccessIsFalse_ShouldWriteDebugLogAndSetDischargeResultErrors()
  //  {
  //    // Arrange.
  //    string ubrnToTest = Generators.GenerateUbrnGp(new Random());
  //    string expectedDebugMessage1 = "Check RECORD_REVIEW_OUTCOME action " +
  //      "is available before setting a review comment.";
  //    string expectedDebugMessage2 = "There was an error getting the" +
  //      " available actions, nothing done to record.";
  //    string expectedErrorMessage = "Error checking for available" +
  //        " actions. Cannot continue.";
  //    bool reportMode = false;

  //    _ersReferralMock.Object.Id = ubrnToTest;
  //    _dischargeMock.Object.Ubrn = ubrnToTest;
  //    _dataProviderMock
  //      .Setup(d => d.GetAvailableActions(
  //        It.IsAny<IErsSession>(),
  //        It.IsAny<IErsReferral>(),
  //        It.IsAny<string>()))
  //      .ReturnsAsync(new AvailableActionResult { Success = false });

  //    _commentResultMock.Setup(t => t.Errors)
  //      .Returns(new List<string>());

  //    // Act.
  //    IReviewCommentResult result = await RecordReviewOutcome(
  //      _commentResultMock.Object,
  //      _dischargeMock.Object,
  //      _dischargeResultMock.Object,
  //      _ersReferralMock.Object,
  //      reportMode);

  //    // Assert.
  //    using (new AssertionScope())
  //    {
  //      _loggerMock.Verify(t => t.Debug(expectedDebugMessage1), Times.Once);
  //      _loggerMock.Verify(t => t.Debug(expectedDebugMessage2), Times.Once);

  //      _dischargeResultMock.Object.Success.Should().BeFalse();
  //      result.Success.Should().BeFalse();
  //      result.Errors.Should().Contain(expectedErrorMessage);
  //    }
  //  }

  //  [Fact]
  //  public async Task WhenAvailableActionIsNot_RECORD_REVIEW_OUTCOME_ShouldWriteDebugLogAndSetDischargeResultSuccessTrue()
  //  {
  //    // Arrange.
  //    string ubrnToTest = Generators.GenerateUbrnGp(new Random());
  //    string expectedDebugMessage1 = "Check RECORD_REVIEW_OUTCOME action is" +
  //      " available before setting a review comment.";
  //    string expectedDebugMessage2 = "Skipped Updating eRS Record for UBRN" +
  //      " {ubrn} as the RECORD_REVIEW_OUTCOME action was not available." +
  //      " WMS record still updated.";
  //    bool reportMode = false;

  //    _availableActionsMock
  //      .Setup(t => t.Contains(
  //        It.IsAny<ReferralAction>()))
  //      .Returns(false);
  //    _ersReferralMock
  //      .Setup(t => t.Id)
  //      .Returns(ubrnToTest);
  //    _dischargeMock.Object.Ubrn = ubrnToTest;
  //    _dataProviderMock
  //      .Setup(d => d.GetAvailableActions(
  //        It.IsAny<IErsSession>(),
  //        It.IsAny<IErsReferral>(),
  //        It.IsAny<string>()))
  //      .ReturnsAsync(new AvailableActionResult
  //      {
  //        Success = true,
  //        Actions = _availableActionsMock.Object
  //      });

  //    // Act.
  //    IReviewCommentResult result = await RecordReviewOutcome(
  //      _commentResultMock.Object,
  //      _dischargeMock.Object,
  //      _dischargeResultMock.Object,
  //      _ersReferralMock.Object,
  //      reportMode);

  //    // Assert.
  //    using (new AssertionScope())
  //    {
  //      _loggerMock.Verify(t => t.Debug(expectedDebugMessage1), Times.Once);
  //      _loggerMock.Verify(t => t.Debug(expectedDebugMessage2, ubrnToTest),
  //        Times.Once);

  //      _dischargeResultMock.Object.Success.Should().BeFalse();
  //      result.Success.Should().BeTrue();
  //    }
  //  }

  //  [Fact]
  //  public async Task WhenAvailableActionIs_RECORD_REVIEW_OUTCOME_ShouldWriteDebugLogAndSetDischargeResultSuccessTrue()
  //  {
  //    // Arrange.
  //    string ubrnToTest = Generators.GenerateUbrnGp(new Random());
  //    string version = "v1.0";
  //    string expectedDebugMessage1 = "Check RECORD_REVIEW_OUTCOME action is" +
  //      " available before setting a review comment.";
  //    string expectedDebugMessage2 = "Updating eRS Record for UBRN {ubrn}" +
  //      " with version {version}.";
  //    bool reportMode = false;

  //    _availableActionsMock
  //      .Setup(t => t.Contains(
  //        It.IsAny<ReferralAction>()))
  //      .Returns(true);

  //    _ersReferralMock
  //      .Setup(t => t.Id)
  //      .Returns(ubrnToTest);

  //    _ersReferralMock
  //      .Setup(t => t.Meta)
  //      .Returns(new ReferralMetaData()
  //      {
  //        versionId = version
  //      });

  //    _dischargeMock.Object.Ubrn = ubrnToTest;
  //    _dataProviderMock
  //      .Setup(d => d.GetAvailableActions(
  //        It.IsAny<IErsSession>(),
  //        It.IsAny<IErsReferral>(),
  //        It.IsAny<string>()))
  //      .ReturnsAsync(new AvailableActionResult
  //      {
  //        Success = true,
  //        Actions = _availableActionsMock.Object
  //      });
  //    _dataProviderMock
  //      .Setup(t => t.RecordOutcome(
  //        It.IsAny<string>(),
  //        It.IsAny<IErsReferral>(),
  //        It.IsAny<string>(),
  //        It.IsAny<Outcome>(),
  //        It.IsAny<IErsSession>()))
  //      .ReturnsAsync(new ReviewCommentResult() { Success = true });

  //    // Act.
  //    IReviewCommentResult result = await RecordReviewOutcome(
  //      _commentResultMock.Object,
  //      _dischargeMock.Object,
  //      _dischargeResultMock.Object,
  //      _ersReferralMock.Object,
  //      reportMode);

  //    // Assert.
  //    using (new AssertionScope())
  //    {
  //      _loggerMock.Verify(t => t.Debug(expectedDebugMessage1), Times.Once);
  //      _loggerMock.Verify(t => t.Debug(
  //        expectedDebugMessage2,
  //        ubrnToTest,
  //        version),
  //        Times.Once);

  //      _dischargeResultMock.Object.Success.Should().BeFalse();
  //      result.Success.Should().BeTrue();
  //    }
  //  }
  //}
}
