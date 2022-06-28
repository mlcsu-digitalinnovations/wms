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

namespace WmsHub.ReferralsService.Tests
{

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
        //Arrange
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
              ReferralAttachmentId = 100
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
        //Act
        var result = await _classToTest.Process(true);
        //Assert
        // TODO This test needs an implementation
      }

      [Fact] //TODO: #1620 Bug
      public async Task ProcessReportOnly_NewRecord_NoAttachmentId()
      {
        //Arrange
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
        _mockProvider
          .Setup(t => t.GetRegistration(
            It.IsAny<ErsWorkListEntry>(),
            It.IsAny<long?>(),
            It.IsAny<long?>(),
            It.IsAny<ErsSession>(),
            false))
          .ReturnsAsync(ersReferralResult);
        _mockProvider.Setup(t =>
            t.UpdateMissingAttachment(It.IsAny<ReferralMissingAttachmentPost>()))
          .Verifiable();
        _classToTest = new ReferralProcessor(_mockProvider.Object,
          _mockSmartCardAuthenticator.Object, _config, _mockLogger.Object, 
          _mockLogger.Object);
        //Act
        ProcessExecutionResult result = await _classToTest.Process(false);
        //Assert
        result.Success.Should().BeTrue();
      }

      [Fact]
      public async Task Process_NewRecord_NhsNumberMissmatch()
      {
        //Arrange
        Mock<ReferralPost> mockReferralPost = new Mock<ReferralPost>();
        mockReferralPost.Object.NhsNumber = "123456789124";

        Mock<ReferralAttachmentPdfProcessor> mockPdf =
          new Mock<ReferralAttachmentPdfProcessor>(_mockLogger.Object,
          _mockLogger.Object, null, null, null);
        mockPdf.Setup(t =>
          t.GenerateReferralCreationObject(It.IsAny<string>(),
            It.IsAny<long?>(), It.IsAny<long?>()))
          .Returns(mockReferralPost.Object);

        Mock<ErsReferralResult> mockErsReferralResult = new();
        mockErsReferralResult.Setup(t => t.Success).Returns(true);
        mockErsReferralResult.Object.NoValidAttachmentFound = false;
        mockErsReferralResult.Object.AttachmentId = 123;
        mockErsReferralResult.Object.Pdf = mockPdf.Object;

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
        _mockProvider.Setup(t => t
          .GetRegistration(
            It.IsAny<ErsWorkListEntry>(),
            It.IsAny<long?>(),
            It.IsAny<long?>(),
            It.IsAny<ErsSession>(),
            false))
          .ReturnsAsync(mockErsReferralResult.Object);
        _mockProvider.Setup(t =>
            t.UpdateNhsNumberMismatch(
              It.IsAny<ReferralNhsNumberMismatchPost>()))
          .Verifiable();
        _classToTest = new ReferralProcessor(_mockProvider.Object,
          _mockSmartCardAuthenticator.Object, _config, _mockLogger.Object, 
          _mockLogger.Object);
        //Act
        ProcessExecutionResult result = await _classToTest.Process(false);
        //Assert
        result.Success.Should().BeTrue();
      }


      [Fact]
      public async Task Process_ExistingRecord_NhsNumberMissmatch()
      {
        //Arrange
        _mockERSWorkListItem.Object.Reference = "type/123456789124";
        Mock<ReferralPut> mockReferralPut = new Mock<ReferralPut>();
        mockReferralPut.Object.NhsNumber = "123456789126";

        Mock<ReferralAttachmentPdfProcessor> mockPdf =
          new Mock<ReferralAttachmentPdfProcessor>(_mockLogger.Object, 
          _mockLogger.Object, null, null, null);
        mockPdf.Setup(t =>
          t.GenerateReferralUpdateObject(
            It.IsAny<string>(),
            It.IsAny<long?>(), 
            It.IsAny<long?>()))
          .Returns(mockReferralPut.Object);

        Mock<ErsReferralResult> mockErsReferralResult = new();
        mockErsReferralResult.Setup(t => t.Success).Returns(true);
        mockErsReferralResult.Object.NoValidAttachmentFound = false;
        mockErsReferralResult.Object.AttachmentId = 123;
        mockErsReferralResult.Object.Pdf = mockPdf.Object;

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
              Status = "AWAITINGUPDATE",
              Ubrn = "123456789124",
              ReferralAttachmentId = 124
            }
          }
        };
        _session = new ErsSession()
        {
          Id = "124",
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
        _mockProvider.Setup(t => t
          .GetRegistration(
            It.IsAny<ErsWorkListEntry>(),
            It.IsAny<long?>(),
            It.IsAny<long?>(),
            It.IsAny<ErsSession>(),
            false))
          .ReturnsAsync(mockErsReferralResult.Object);
        _mockProvider.Setup(t =>
            t.UpdateNhsNumberMismatch(
              It.IsAny<ReferralNhsNumberMismatchPost>()))
          .Verifiable();
        _classToTest = new ReferralProcessor(_mockProvider.Object,
          _mockSmartCardAuthenticator.Object, _config, _mockLogger.Object, 
          _mockLogger.Object);
        //Act
        ProcessExecutionResult result = await _classToTest.Process(false);
        //Assert
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

        RegistrationList testRegistrationList = new RegistrationList()
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

      public ReferralService_ProcessorTests() :
        base(_mockDataProvider.Object,
          _mockSmartCardAuthenticator.Object,
          null,
          _mockLogger.Object, _mockLogger.Object)
      {
      }

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
            Entry = new[] {entry}
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
          return 1;
        }

        protected override async Task<bool> ProcessNewReferral(
          ErsWorkListEntry workListEntry,
          bool reportOnly, bool showDiagnostics, long? attachmentId = null)
        {
          return false;
        }

        public override async Task<bool> ProcessExistingReferral(
          GetActiveUbrnResponse wmsRecord,
          ErsWorkListEntry ersRecord,
          bool reportOnly,
          bool showDiagnostics,
          long? overrideAttachmentId,
          bool reprocessUnchangedAttachment)
        {

          if (wmsRecord.ReferralAttachmentId == 101)
          {
            Assert.True(true, "wmsRecord == null");
            throw new ArgumentNullException(
              "Active Ubrn Record should not be null");
          }

          if (wmsRecord.ReferralAttachmentId == 102)
          {
            Assert.True(true, "ersRecord == null");
            throw new ArgumentNullException(
              "Ers Worklist Entry should not be null");
          }

          if (wmsRecord.ReferralAttachmentId == 103)
          {
            Assert.True(true, "DataProvider Errors");
            return false;
          }

          if (wmsRecord.ReferralAttachmentId == 104)
          {
            Assert.True(true,
              "registrationRecord.AttachmentId == " +
              "wmsRecord.ReferralAttachmentId");
            return true;
          }

          if (reportOnly)
          {
            return true;
          }


          if (wmsRecord.ReferralAttachmentId == 105)
          {
            Assert.True(true, "updateReferralResult.Success == false");
            return false;
          }

          return true;
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
          //arrange
          string expected = string.Format(_error, "Batch was null.");
          //act
          try
          {
            ProcessExecutionResult response = await Process(batch: null, false);
            //assert
            throw new Exception("Expected exception not returned");
          }
          catch (ArgumentNullException ex)
          {
            Assert.True(true, ex.Message);
            ex.Message.Should().Be(expected);
          }
          catch (Exception ex)
          {
            Assert.True(false, ex.Message);

          }
        }

        [Fact]
        public async Task BatchItems_Null_ArgumentNullException_Expected()
        {
          //arrange
          string expected = string.Format(_error, "Batch contained no items.");
          Batch batch = new Batch();
          //act
          try
          {
            ProcessExecutionResult response = await Process(batch: batch, false);
            //assert
            throw new Exception("Expected exception not returned");
          }
          catch (ArgumentNullException ex)
          {
            Assert.True(true, ex.Message);
            ex.Message.Should().Be(expected);
          }
          catch (Exception ex)
          {
            Assert.True(false, ex.Message);
          }
        }

        [Fact]
        public async Task BatchItems_0_InvalidDataException_Expected()
        {
          //arrange
          string expected = "Batch was empty.";
          Batch batch = new Batch();
          batch.Items = new List<BatchItem>();
          //act
          try
          {
            ProcessExecutionResult response = await Process(batch: batch, false);
            //assert
            throw new Exception("Expected exception not returned");
          }
          catch (InvalidDataException ex)
          {
            Assert.True(true, ex.Message);
          }
          catch (Exception ex)
          {
            Assert.True(false, ex.Message);
            ex.Message.Should().Be(expected);
          }
        }



      }



    }

  }
}