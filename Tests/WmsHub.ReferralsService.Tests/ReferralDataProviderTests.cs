using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Moq.Protected;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WmsHub.Business.Exceptions;
using WmsHub.Common.Interface;
using WmsHub.Common.Models;
using WmsHub.ReferralsService.Interfaces;
using WmsHub.ReferralsService.Models.Configuration;
using WmsHub.ReferralsService.Models.Results;
using Xunit;

namespace WmsHub.ReferralsService.Tests;

public class ReferralDataProviderTests
{
  private MockReferralsDataProvider _classToTest;
  private Config _config;
  private readonly Mock<ILogger> _mockLogger = new();
  private readonly Mock<ILogger> _mockAuditLogger = new();
  private readonly Mock<ISmartCardAuthentictor> _mockSmartCardAuthenticator = new();
  private readonly Mock<IHttpClientFactory> _mockHttpClientFactory = new();

  public ReferralDataProviderTests()
  {
    _config = new Config
    {
      Data = new DataConfiguration()
      {
        ExcludedFiles = new() { "obs" }
      }
    };
  }

  public class GetRegistrationTests : ReferralDataProviderTests
  {
    private Mock<ErsSession> _mockSession = new();
    private Mock<WorkListResult> _mockWorkListResult = new();
    private Mock<ErsWorkList> _mockErsWorkList = new();
    private Mock<ErsWorkListEntry> _mockErsWorkListEntry = new();
    private Mock<ERSWorkListItem> _mockERSWorkListItem = new();

    public GetRegistrationTests()
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

    [Fact]
    public async Task GetRegistration_A006_Exception_Success_false()
    {
      // Arrange.
      _classToTest = new MockReferralsDataProvider(_config,
        _mockHttpClientFactory.Object,
        _mockSmartCardAuthenticator.Object,
        _mockLogger.Object,
        _mockAuditLogger.Object,
        throwException: true,
        returnFalse: true
        );

      // Act.
      ErsReferralResult result = await _classToTest.GetRegistration(
        _mockErsWorkListEntry.Object,
        "101",
        "101",
        null,
        _mockSession.Object);

      // Assert.
      result.WasRetrievedFromErs.Should().BeFalse();
    }

    [Fact]
    public async Task GetRegistration_A006_InvalidExtension_Success_false()
    {
      // Arrange.
      _classToTest = new MockReferralsDataProvider(_config,
        _mockHttpClientFactory.Object,
        _mockSmartCardAuthenticator.Object,
        _mockLogger.Object,
        _mockAuditLogger.Object,
        throwException: false,
        throwIncorrectFileTypeException: true,
        returnFalse: true
        );

      // Act.
      ErsReferralResult result = await _classToTest.GetRegistration(
        _mockErsWorkListEntry.Object,
        "101",
        "101",
        null,
        _mockSession.Object);

      // Assert.
      using (new AssertionScope())
      {
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
      }
    }
  }

  public class MockReferralsDataProvider : ReferralsDataProvider
  {
    private Config _config;
    private Mock<ErsAttachment> _mockAttachment = new();
    private Mock<IErsReferral> _mockReferral = new();
    private ErsReferralResult _referralResult = new();
    private bool _returnFalse = false;
    private bool _throwException = false;
    private bool _throwIncorrectFileTypeException = false;

    public MockReferralsDataProvider(
      Config configuration,
      IHttpClientFactory httpClientFactory,
      ISmartCardAuthentictor smartCardAuthentication,
      ILogger log,
      ILogger auditLog = null,
      bool throwException = false,
      bool throwIncorrectFileTypeException = false,
      bool returnFalse = false)
      : base(configuration, httpClientFactory, smartCardAuthentication, log, auditLog)
    {
      _throwException = throwException;
      _throwIncorrectFileTypeException = throwIncorrectFileTypeException;
      _returnFalse = returnFalse;
      _config = new Config
      {
        Data = new DataConfiguration()
        {
          ExcludedFiles = new() { "obs" },
          InteropTemporaryFilePath = "test",
          AccreditedSystemsID = "test",
          Fqdn = "test",
          ReformatDocument = false,
          SectionHeadings = new[] { "test" },
          RetryAllSources = false,
          NumberOfMissingQuestionsTolerance = 1
        }
      };
    }

    public override HttpClient GetHttpClientWithClientCertificate()
    {
      Mock<HttpMessageHandler> mockHandler = new();

      if (_throwException)
      {
        mockHandler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
          .Throws(new TimeoutException("test"));
      }
      else
      {
        mockHandler
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(
           new HttpResponseMessage(HttpStatusCode.OK));
      }

      return new HttpClient(mockHandler.Object)
      {
        BaseAddress = new Uri("http://www.test.com")
      };
    }

    public override async Task<ErsReferralResult> GetRegistration(
      ErsWorkListEntry ersWorkListEntry,
      string attachmentId,
      string overrideAttachmentId,
      DateTimeOffset? mostRecentAttachmentDate,
      ErsSession activeSession,
      bool showDiagnostics = false) => await base.GetRegistration(
        ersWorkListEntry,
        attachmentId,
        overrideAttachmentId,
        mostRecentAttachmentDate,
        activeSession,
        showDiagnostics);


    public override Task<IErsReferral> GetErsReferralByUbrn(
      IErsSession session,
      string ubrn,
      string nhsNumber)
    {
      ErsAttachment attachment = new();
      attachment.Id = "101";
      attachment.Title = "exception.exe";
      List<ErsAttachment> attachments = new()
        {
          attachment
        };
      _mockReferral.Setup(t => t.Attachments).Returns(attachments);

      return Task.FromResult(_mockReferral.Object);
    }
  }
}
