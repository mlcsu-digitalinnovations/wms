using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Exceptions;
using WmsHub.Business.Helpers;
using WmsHub.Business.Models.GpDocumentProxy;
using WmsHub.Business.Services;
using WmsHub.Business.Tests.Helpers;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;
using static WmsHub.Common.Helpers.Constants;

namespace WmsHub.Business.Tests.Services;
public partial class ReferralServiceTests : ServiceTestsBase
{
  public class UpdateDischargesTests : ReferralServiceTests, IDisposable
  {
    private readonly Mock<GpDocumentProxyOptions> _gpDocumentProxyOptionsMock = new();
    private readonly HttpClient _httpClient;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IOptions<GpDocumentProxyOptions>> _optionsMock = new();
    private new readonly ReferralService _service;
    private const string UpdateDischargesExceptionMessage =
      "Update Discharges ran with errors, latest error: ";

    public UpdateDischargesTests(
        ServiceFixture serviceFixture,
        ITestOutputHelper testOutputHelper)
        : base(serviceFixture, testOutputHelper)
    {
      GpDocumentProxyOptions options = TestConfiguration.CreateGpDocumentProxyOptions();
      _gpDocumentProxyOptionsMock.Object.Gp = options.Gp;
      _gpDocumentProxyOptionsMock.Object.Endpoint = "https://localtest.com/document";
      _gpDocumentProxyOptionsMock.Object.Msk = options.Msk;
      _gpDocumentProxyOptionsMock.Object.DelayEndpoint = "/delay/";
      _gpDocumentProxyOptionsMock.Object.ResolveEndpoint = "/resolve/";
      _gpDocumentProxyOptionsMock.Object.UpdateEndpoint = "/update/";
      _gpDocumentProxyOptionsMock.Object.AwaitingDischargeRejectionReasons =
        options.AwaitingDischargeRejectionReasons;
      _gpDocumentProxyOptionsMock.Object.CompleteRejectionReasons = 
        options.CompleteRejectionReasons;
      _gpDocumentProxyOptionsMock.Object.GpdpCompleteRejectionReasons =
        options.GpdpCompleteRejectionReasons;
      _gpDocumentProxyOptionsMock.Object.GpdpTracePatientRejectionReasons =
        options.GpdpTracePatientRejectionReasons;
      _gpDocumentProxyOptionsMock.Object.GpdpUnableToDischargeRejectionReasons =
        options.GpdpUnableToDischargeRejectionReasons;
      _gpDocumentProxyOptionsMock.Object.TracePatientRejectionReasons =
        options.TracePatientRejectionReasons;
      _gpDocumentProxyOptionsMock.Object.UnableToDischargeRejectionReasons =
        options.UnableToDischargeRejectionReasons;
      

      _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
      _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
      {
        BaseAddress = new Uri("http://test.com/")
      };

      _mockLogger = new Mock<ILogger>();

      _optionsMock.Setup(t => t.Value)
        .Returns(_gpDocumentProxyOptionsMock.Object);

      _service = new ReferralService(
        _context,
        _serviceFixture.Mapper,
        null, // provider service
        _mockDeprivationService.Object,
        _mockLinkIdService.Object,
        _mockPostcodeIoService.Object,
        _mockPatientTriageService.Object,
        _mockOdsOrganisationService.Object,
        _optionsMock.Object,
        _mockReferralTimelineOptions.Object,
        _httpClient,
        _mockLogger.Object)
      {
        User = GetClaimsPrincipal()
      };
    }

    public new void Dispose()
    {
      _context.Referrals.RemoveRange(_context.Referrals);
      _context.SaveChanges();
      GC.SuppressFinalize(this);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Acceptance overdue. Auto accepted after 90 days.")]
    public async Task DischargeAccepted(string statusReason)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.SentForDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Accepted.ToString(),
        Information = statusReason,
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);

      // Act.
      GpDocumentProxyUpdateResponse result = await _service.UpdateDischarges();
      Referral testReferral = await _context.Referrals.SingleAsync(t => t.Id == referral.Id);

      // Assert.
      result.CountOfComplete.Should().Be(1);
      result.CountOfUpdated.Should().Be(1);
      result.Discharges.Should().HaveCount(1);
      result.Discharges[0].Status.Should().Be(ReferralStatus.Complete.ToString());
      result.Discharges[0].Ubrn.Should().Be(referral.Ubrn);
      result.Discharges[0].UpdateStatus.Should().Be(DocumentUpdateStatus.Updated.ToString());
      testReferral.Status.Should().Be(ReferralStatus.Complete.ToString());
      testReferral.StatusReason.Should().Be(statusReason);
    }

    [Fact]
    public async Task DischargeNonMatchingProgrammeOutcomeError()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: "Invalid");
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.OrganisationNotSupported.ToString(),
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.NotUpdated.ToString()
      };
      string messageTemplate =
        "Update Discharge for referral {Id} failed: {message}.";
      string message = $"Programme Outcome {referral.ProgrammeOutcome} does not match any " +
        "valid programme outcomes.";
      string exceptionMessage = $"{UpdateDischargesExceptionMessage}Update Discharge for " +
        $"referral {referral.Id} failed: {message}.";
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(exceptionMessage);
      _mockLogger.Verify(t => t.Error(messageTemplate, referral.Id, message), Times.Once);
    }

    [Fact]
    public async Task UnauthorizedException()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.SentForDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      SetHttpMessageHttpHandlerMockUpdateNoContent(HttpStatusCode.Unauthorized);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<InvalidTokenException>();
    }

    [Fact]
    public async Task InternalSystemErrorException()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.SentForDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      string error = "Test Exception";
      string exceptionMessage = $"{UpdateDischargesExceptionMessage}{error}";
      SetHttpMessageHttpHandlerMockUpdateException(error);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(exceptionMessage);
      _mockLogger.Verify(t => t.Error(It.IsAny<Exception>(), error), Times.Once);
    }

    [Fact]
    public async Task DischargeNonMatchingUpdateStatusError()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString());
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = "Invalid",
        ReferralId = referral.Id,
        UpdateStatus = "Invalid"
      };
      Guid templateId = _gpDocumentProxyOptionsMock.Object.Gp.ProgrammeOutcomeCompleteTemplateId;
      string messageTemplate = "Update Discharge for referral {Id} with template {templateId} " + 
        "returned invalid UpdateStatus: {UpdateStatus}.";
      string exceptionMessage = $"{UpdateDischargesExceptionMessage}Update Discharge for " + 
        $"referral {referral.Id} with template {templateId} returned invalid UpdateStatus: " + 
        $"{data.UpdateStatus}.";
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(exceptionMessage);
      _mockLogger.Verify(t =>
        t.Error(messageTemplate, referral.Id, templateId, data.UpdateStatus),
        Times.Once);
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task DischargeOrganisationNotSupported(ReferralSource referralSource)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: referralSource);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.OrganisationNotSupported.ToString(),
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.NotUpdated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);

      // Act.
      GpDocumentProxyUpdateResponse result = await _service.UpdateDischarges();
      Referral testReferral =
       await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      List<ReferralAudit> testReferralAudits = _context.ReferralsAudit
        .Where(t => t.Ubrn == testReferral.Ubrn)
        .OrderBy(t => t.AuditId).ToList();

      // Assert.
      using (new AssertionScope())
      {
        result.CountOfNotUpdated.Should().Be(1);
        result.CountOfComplete.Should().Be(1);
        result.Discharges.Should().HaveCount(1);
        result.Discharges[0].Status.Should()
          .Be(ReferralStatus.Complete.ToString());
        result.Discharges[0].Ubrn.Should().Be(referral.Ubrn);
        result.Discharges[0].UpdateStatus.Should()
          .Be(DocumentUpdateStatus.NotUpdated.ToString());
        testReferral.Status.Should().Be(ReferralStatus.Complete.ToString());
        testReferralAudits.Last().Status.Should()
          .Be(ReferralStatus.Complete.ToString());
        testReferralAudits.Last().StatusReason.Should().BeNull();
        testReferralAudits[^2].Status.Should()
          .Be(ReferralStatus.UnableToDischarge.ToString());
        testReferralAudits[^2].StatusReason.Should()
          .Be(ORGANISATION_NOT_SUPPORTED);
      }
    }

    [Fact]
    public async Task DischargeResponseBodyNullError()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString());
      AddAndDetatchReferral(referral);
      Guid templateId = _gpDocumentProxyOptionsMock.Object.Gp.ProgrammeOutcomeCompleteTemplateId;
      string messageTemplate = "Update Discharge for referral {Id} with template {templateId} " + 
        "returned 200 OK with empty request body.";
      string message = $"{UpdateDischargesExceptionMessage}Update Discharge for referral " + 
        $"{referral.Id} with template {templateId} returned 200 OK with empty request body.";
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(null)),
        HttpStatusCode.OK);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(message);
      _mockLogger.Verify(t => t.Error(messageTemplate, referral.Id, templateId), Times.Once);
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task DischargePending(ReferralSource referralSource)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: referralSource);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.DischargePending.ToString(),
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.NotUpdated.ToString()
      };
      string messageTemplate = "Discharge for referral {Id} is waiting to be sent.";
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);

      // Act.
      GpDocumentProxyUpdateResponse result = await _service.UpdateDischarges();

      // Assert.
      result.CountOfNotUpdated.Should().Be(1);
      result.CountOfSentForDischarge.Should().Be(1);
      result.Discharges.Should().HaveCount(1);
      result.Discharges[0].Status.Should().Be(ReferralStatus.SentForDischarge.ToString());
      result.Discharges[0].Ubrn.Should().Be(referral.Ubrn);
      result.Discharges[0].UpdateStatus.Should().Be(DocumentUpdateStatus.NotUpdated.ToString());
      _mockLogger.Verify(t => t.Information(messageTemplate, referral.Id), Times.Once);
    }

    [Theory]
    [MemberData(nameof(DischargeAwaitingTraceRejectionReasons))]
    public async Task RejectedDischargeAwaitingTrace(
      ReferralSource referralSource,
      string rejectionReason)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: referralSource);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = rejectionReason,
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      SetHttpMessageHttpHandlerMockResolve();

      // Act.
      GpDocumentProxyUpdateResponse result = await _service.UpdateDischarges();
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      List<ReferralAudit> testReferralAudits = _context.ReferralsAudit
        .Where(t => t.Ubrn == testReferral.Ubrn)
        .OrderBy(t => t.AuditId).ToList();

      // Assert.
      result.CountOfUpdated.Should().Be(1);
      result.CountOfDischargeAwaitingTrace.Should().Be(1);
      result.Discharges.Should().HaveCount(1);
      result.Discharges[0].Status.Should().Be(ReferralStatus.DischargeAwaitingTrace.ToString());
      result.Discharges[0].Ubrn.Should().Be(referral.Ubrn);
      result.Discharges[0].UpdateStatus.Should().Be(DocumentUpdateStatus.Updated.ToString());
      testReferral.Status.Should().Be(ReferralStatus.DischargeAwaitingTrace.ToString());
      testReferralAudits.Last().Status.Should()
        .Be(ReferralStatus.DischargeAwaitingTrace.ToString());
      testReferralAudits.Last().StatusReason.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(DischargeAwaitingTraceRejectionReasons))]

    public async Task RejectedDischargeAwaitingTraceUnsuccessful(
      ReferralSource referralSource,
      string rejectionReason)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: referralSource);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = rejectionReason,
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;
      SetHttpMessageHttpHandlerMockResolve(expectedStatusCode);
      string errorTemplate = "Resolve Document Rejection for referral {Id} returned status " +
        "{StatusCode}.";
      string message = $"{UpdateDischargesExceptionMessage}Resolve Document Rejection for " + 
        $"referral {referral.Id} returned status {expectedStatusCode}.";

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(message);
      _mockLogger.Verify(t => t.Error(errorTemplate, referral.Id, expectedStatusCode));
    }

    [Theory]
    [MemberData(nameof(AwaitingDischargeRejectionReasons))]

    public async Task RejectedAwaitingDischarge(string rejectionReason)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.Msk);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = rejectionReason,
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      SetHttpMessageHttpHandlerMockResolve();

      // Act.
      GpDocumentProxyUpdateResponse result = await _service.UpdateDischarges();
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);

      // Assert.
      result.CountOfUpdated.Should().Be(1);
      result.CountOfAwaitingDischarge.Should().Be(1);
      result.Discharges.Should().HaveCount(1);
      result.Discharges[0].Status.Should().Be(ReferralStatus.AwaitingDischarge.ToString());
      result.Discharges[0].Ubrn.Should().Be(referral.Ubrn);
      result.Discharges[0].UpdateStatus.Should().Be(DocumentUpdateStatus.Updated.ToString());
      testReferral.Status.Should().Be(ReferralStatus.AwaitingDischarge.ToString());
      testReferral.StatusReason.Should().Be(rejectionReason);
    }

    [Theory]
    [MemberData(nameof(AwaitingDischargeRejectionReasons))]

    public async Task RejectedAwaitingDischargeUnsuccessful(string rejectionReason)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = rejectionReason,
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;
      string errorTemplate = "Resolve Document Rejection for referral {Id} returned status " +
        "{StatusCode}.";
      string error = $"Resolve Document Rejection for referral {referral.Id} returned status " +
        $"{expectedStatusCode}.";
      string message = $"{UpdateDischargesExceptionMessage}{error}";
      SetHttpMessageHttpHandlerMockResolve(expectedStatusCode);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(message);
      _mockLogger.Verify(t => t.Error(errorTemplate, referral.Id, expectedStatusCode));
    }

    [Theory]
    [MemberData(nameof(CompleteRejectionReasons))]

    public async Task RejectedComplete(string rejectionReason)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.Msk);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = rejectionReason,
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      SetHttpMessageHttpHandlerMockResolve();

      // Act.
      GpDocumentProxyUpdateResponse result = await _service.UpdateDischarges();
      Referral testReferral =
        await _context.Referrals.SingleAsync(t => t.Id == referral.Id);

      // Assert.
      result.CountOfUpdated.Should().Be(1);
      result.CountOfComplete.Should().Be(1);
      result.Discharges.Should().HaveCount(1);
      result.Discharges[0].Status.Should().Be(ReferralStatus.Complete.ToString());
      result.Discharges[0].Ubrn.Should().Be(referral.Ubrn);
      result.Discharges[0].UpdateStatus.Should().Be(DocumentUpdateStatus.Updated.ToString());
      testReferral.Status.Should().Be(ReferralStatus.Complete.ToString());
      testReferral.StatusReason.Should().Be(rejectionReason);
    }

    [Theory]
    [MemberData(nameof(CompleteRejectionReasons))]

    public async Task RejectedCompleteUnsuccessful(string rejectionReason)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = rejectionReason,
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;
      string errorTemplate = "Resolve Document Rejection for referral {Id} returned status " +
        "{StatusCode}.";
      string error = $"Resolve Document Rejection for referral {referral.Id} returned status " + 
        $"{expectedStatusCode}.";
      string message = $"{UpdateDischargesExceptionMessage}{error}";
      SetHttpMessageHttpHandlerMockResolve(expectedStatusCode);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(message);
      _mockLogger.Verify(t => t.Error(errorTemplate, referral.Id, expectedStatusCode));
    }

    [Theory]
    [MemberData(nameof(UnableToDischargeRejectionReasons))]

    public async Task RejectedUnableToDischarge(string rejectionReason)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.Msk);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = rejectionReason,
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      SetHttpMessageHttpHandlerMockResolve();

      // Act.
      GpDocumentProxyUpdateResponse result = await _service.UpdateDischarges();
      Referral testReferral =
       await _context.Referrals.SingleAsync(t => t.Id == referral.Id);
      List<ReferralAudit> testReferralAudits = _context.ReferralsAudit
        .Where(t => t.Ubrn == testReferral.Ubrn)
        .OrderBy(t => t.AuditId).ToList();

      // Assert.
      result.CountOfUpdated.Should().Be(1);
      result.CountOfComplete.Should().Be(1);
      result.Discharges.Should().HaveCount(1);
      result.Discharges[0].Status.Should().Be(ReferralStatus.Complete.ToString());
      result.Discharges[0].Ubrn.Should().Be(referral.Ubrn);
      result.Discharges[0].UpdateStatus.Should().Be(DocumentUpdateStatus.Updated.ToString());
      testReferral.Status.Should().Be(ReferralStatus.Complete.ToString());
      testReferral.StatusReason.Should().BeNull();
      testReferralAudits.Last().Status.Should().Be(ReferralStatus.Complete.ToString());
      testReferralAudits.Last().StatusReason.Should().BeNull();
      testReferralAudits[^2].Status.Should().Be(ReferralStatus.UnableToDischarge.ToString());
      testReferralAudits[^2].StatusReason.Should().Be(rejectionReason);
    }

    [Theory]
    [MemberData(nameof(UnableToDischargeRejectionReasons))]

    public async Task RejectedUnableToDischargeUnsuccessful(string rejectionReason)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = rejectionReason,
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;
      SetHttpMessageHttpHandlerMockResolve(expectedStatusCode);
      string errorTemplate = "Resolve Document Rejection for referral {Id} returned status " +
        "{StatusCode}.";
      string message = $"{UpdateDischargesExceptionMessage}Resolve Document Rejection for " + 
        $"referral {referral.Id} returned status {expectedStatusCode}.";

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(message);
      _mockLogger.Verify(t => t.Error(errorTemplate, referral.Id, expectedStatusCode));
    }

    [Fact]
    public async Task RejectedDelayOk()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = "INVALIDREJECTIONREASON",
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      SetHttpMessageHttpHandlerMockDelay(HttpStatusCode.OK);

      // Act.
      GpDocumentProxyUpdateResponse result = await _service.UpdateDischarges();
      Referral testReferral = await _context.Referrals.SingleAsync(t => t.Id == referral.Id);

      // Assert.
      result.CountOfUpdated.Should().Be(1);
      result.CountOfSentForDischarge.Should().Be(1);
      result.Discharges.Should().HaveCount(1);
      result.Discharges[0].Status.Should().Be(ReferralStatus.SentForDischarge.ToString());
      result.Discharges[0].Ubrn.Should().Be(referral.Ubrn);
      result.Discharges[0].UpdateStatus.Should().Be(DocumentUpdateStatus.Updated.ToString());
      testReferral.Status.Should().Be(referral.Status);
      testReferral.StatusReason.Should().BeNull();
    }

    [Fact]
    public async Task RejectedDelayUnauthorized()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = "INVALIDREJECTIONREASON",
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      SetHttpMessageHttpHandlerMockDelay(HttpStatusCode.Unauthorized);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<InvalidTokenException>();
    }

    [Fact]
    public async Task RejectedDelayBadRequest()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = "INVALIDREJECTIONREASON",
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      HttpStatusCode statusCode = HttpStatusCode.BadRequest;
      string messageTemplate = "Set Document Delay for referral {Id} returned status {StatusCode}.";
      string message = $"{UpdateDischargesExceptionMessage}Set Document Delay for referral " + 
        $"{referral.Id} returned status {statusCode}.";
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      SetHttpMessageHttpHandlerMockDelay(statusCode);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(message);
      _mockLogger.Verify(t => t.Error(messageTemplate, referral.Id, statusCode), Times.Once);
    }

    [Fact]
    public async Task RejectedDelayException()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = "INVALIDREJECTIONREASON",
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      string error = "Test Exception";
      string message = $"{UpdateDischargesExceptionMessage}{error}";
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      SetHttpMessageHttpHandlerMockDelayException(error);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(message);
      _mockLogger.Verify(t => t.Error(It.IsAny<Exception>(), error), Times.Once);
    }

    [Fact]
    public async Task DischargeRejectionResolved()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString());
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.RejectionResolved.ToString(),
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);

      // Act.
      GpDocumentProxyUpdateResponse result = await _service.UpdateDischarges();
      Referral testReferral = await _context.Referrals.SingleAsync(t => t.Id == referral.Id);

      // Assert.
      result.CountOfAwaitingDischarge.Should().Be(1);
      result.CountOfUpdated.Should().Be(1);
      result.Discharges.Should().HaveCount(1);
      result.Discharges[0].Status.Should().Be(ReferralStatus.AwaitingDischarge.ToString());
      result.Discharges[0].Ubrn.Should().Be(referral.Ubrn);
      result.Discharges[0].UpdateStatus.Should().Be(DocumentUpdateStatus.Updated.ToString());
      testReferral.Status.Should().Be(ReferralStatus.AwaitingDischarge.ToString());
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task DischargeNoContent(ReferralSource referralSource)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: referralSource);
      AddAndDetatchReferral(referral);
      Guid templateId = _gpDocumentProxyOptionsMock.Object.Gp.ProgrammeOutcomeCompleteTemplateId;
      string messageTemplate = "Update Discharge for referral {Id} with template {templateId} " +
        "returned 204 No Content.";
      string message = $"{UpdateDischargesExceptionMessage}Update Discharge for referral " + 
        $"{referral.Id} with template {templateId} returned 204 No Content.";
      SetHttpMessageHttpHandlerMockUpdate(null, HttpStatusCode.NoContent);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(message);
      _mockLogger.Verify(t => t.Error(messageTemplate, referral.Id, templateId), Times.Once);
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task DischargeBadRequest(ReferralSource referralSource)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: referralSource);
      AddAndDetatchReferral(referral);
      Guid templateId = _gpDocumentProxyOptionsMock.Object.Gp.ProgrammeOutcomeCompleteTemplateId;
      string messageTemplate = "Update Discharge for referral {Id} with template {templateId} " +
        "and UBRN {Ubrn} returned 400 Bad Request.";
      string message = $"{UpdateDischargesExceptionMessage}Update Discharge for referral " + 
        $"{referral.Id} with template {templateId} and UBRN {referral.Ubrn} returned 400 Bad " + 
        "Request.";
      SetHttpMessageHttpHandlerMockUpdate(null, HttpStatusCode.BadRequest);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(message);
      _mockLogger.Verify(t => 
        t.Error(messageTemplate, referral.Id, templateId, referral.Ubrn),
        Times.Once);
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task DischargeOtherError(ReferralSource referralSource)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: referralSource);
      AddAndDetatchReferral(referral);
      Guid templateId = _gpDocumentProxyOptionsMock.Object.Gp.ProgrammeOutcomeCompleteTemplateId;
      HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
      string messageTemplate = "Update Discharge for referral {Id} with template {templateId} " + 
        "returned status {StatusCode}.";
      string message = $"{UpdateDischargesExceptionMessage}Update Discharge for referral " + 
        $"{referral.Id} with template {templateId} returned status {statusCode}.";
      SetHttpMessageHttpHandlerMockUpdate(null, statusCode);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(message);
      _mockLogger.Verify(t =>
        t.Error(messageTemplate, referral.Id, templateId, statusCode),
        Times.Once);
    }

    [Fact]
    public async Task ResolveDocumentRejectionUnauthorizedInvalidTokenException()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = "REJ05",
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      SetHttpMessageHttpHandlerMockResolve(HttpStatusCode.Unauthorized);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<InvalidTokenException>();
    }

    [Fact]
    public async Task ResolveDocumentRejectionOtherStatus()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = "REJ05",
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
      string errorTemplate = "Resolve Document Rejection for referral {Id} returned status " +
        "{StatusCode}.";
      string message = $"{UpdateDischargesExceptionMessage}Resolve Document Rejection for " + 
        $"referral {referral.Id} returned status {statusCode}.";
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      SetHttpMessageHttpHandlerMockResolve(statusCode);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(message);
      _mockLogger.Verify(t => t.Error(errorTemplate, referral.Id, statusCode));
    }

    [Fact]
    public async Task ResolveDocumentRejectionOtherException()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
        status: ReferralStatus.SentForDischarge,
        programmeOutcome: ProgrammeOutcome.Complete.ToString(),
        referralSource: ReferralSource.GpReferral);
      AddAndDetatchReferral(referral);
      GpDocumentProxyDocumentUpdateResponse data = new()
      {
        DocumentStatus = DocumentStatus.Rejected.ToString(),
        Information = "REJ05",
        ReferralId = referral.Id,
        UpdateStatus = DocumentUpdateStatus.Updated.ToString()
      };
      string errorMessage = "Test Exception";
      string exceptionMessage = $"{UpdateDischargesExceptionMessage}{errorMessage}";
      SetHttpMessageHttpHandlerMockUpdate(
        new StringContent(JsonConvert.SerializeObject(data)),
        HttpStatusCode.OK);
      SetHttpMessageHttpHandlerMockResolveException(errorMessage);

      // Act.
      Exception resultException = await Record.ExceptionAsync(_service.UpdateDischarges);

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<UpdateDischargesException>();
      resultException.Message.Should().Be(exceptionMessage);
      _mockLogger.Verify(t => t.Error(It.IsAny<Exception>(), errorMessage));
    }

    private void AddAndDetatchReferral(Referral referral)
    {
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;
    }

    private void SetHttpMessageHttpHandlerMockDelay(HttpStatusCode statusCode)
    {
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath
          .Contains("/delay/")),
        ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = statusCode
        });
    }

    private void SetHttpMessageHttpHandlerMockDelayException(string message)
    {
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath
          .Contains("/delay/")),
        ItExpr.IsAny<CancellationToken>())
        .ThrowsAsync(new UnitTestExpectedExceptionNotThrownException(message));
    }

    private void SetHttpMessageHttpHandlerMockUpdate(
      HttpContent content,
      HttpStatusCode statusCode)
    {
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath
          .Contains("/update/")),
        ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          Content = content,
          StatusCode = statusCode
        });
    }

    private void SetHttpMessageHttpHandlerMockUpdateNoContent(HttpStatusCode statusCode)
    {
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath
          .Contains("/update/")),
        ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = statusCode
        });
    }

    private void SetHttpMessageHttpHandlerMockUpdateException(string message)
    {
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath
          .Contains("/update/")),
        ItExpr.IsAny<CancellationToken>())
        .ThrowsAsync(new UnitTestExpectedExceptionNotThrownException(message));
    }

    private void SetHttpMessageHttpHandlerMockResolve(HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath
          .Contains("/resolve/")),
        ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = statusCode
        });
    }

    private void SetHttpMessageHttpHandlerMockResolveException(string message)
    {
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath
          .Contains("/resolve/")),
        ItExpr.IsAny<CancellationToken>())
          .ThrowsAsync(new UnitTestExpectedExceptionNotThrownException(message));
    }
  }
}