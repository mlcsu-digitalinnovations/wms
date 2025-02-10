using FluentAssertions;
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
using WmsHub.Business.Models.ReferralService;
using WmsHub.Business.Services;
using WmsHub.Business.Tests.Helpers;
using WmsHub.Tests.Helper;
using Xunit;
using Xunit.Abstractions;

namespace WmsHub.Business.Tests.Services;
public partial class ReferralServiceTests : ServiceTestsBase
{
  public class PostDischargesTests : ReferralServiceTests, IDisposable
  {
    private readonly HttpClient _httpClient;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<GpDocumentProxyOptions> _gpDocumentProxyOptionsMock = new();
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IOptions<GpDocumentProxyOptions>> _optionsMock = new();
    private readonly string[] _outcomesRequiringMessage;
    private const string PostDischargesExceptionMessage = 
      "Post Discharges ran with errors, latest error: ";
    private readonly Provider _provider;
    private readonly Mock<IOptions<ReferralTimelineOptions>> _referralTimelineOptionsMock = new();
    private new readonly ReferralService _service;

    public PostDischargesTests(ServiceFixture serviceFixture, ITestOutputHelper testOutputHelper) 
      : base(serviceFixture, testOutputHelper)
    {
      GpDocumentProxyOptions options = TestConfiguration.CreateGpDocumentProxyOptions();
      _gpDocumentProxyOptionsMock.Object.Gp = options.Gp;
      _gpDocumentProxyOptionsMock.Object.Endpoint = "https://localtest.com/discharge";
      _gpDocumentProxyOptionsMock.Object.Msk = options.Msk;
      _gpDocumentProxyOptionsMock.Object.PostEndpoint = "/discharge";
      _gpDocumentProxyOptionsMock.Object.TracePatientRejectionReasons =
        options.TracePatientRejectionReasons;
      _gpDocumentProxyOptionsMock.Object.GpdpTracePatientRejectionReasons =
        options.GpdpTracePatientRejectionReasons;

      _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
      _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
      {
        BaseAddress = new Uri("http://test.com/")
      };

      _mockLogger = new Mock<ILogger>();

      _optionsMock.Setup(t => t.Value)
        .Returns(_gpDocumentProxyOptionsMock.Object);

      _outcomesRequiringMessage = GpDocumentProxyHelper.ProgrammeOutcomesRequiringMessage();

      _provider = RandomEntityCreator.CreateRandomProvider();

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
        _referralTimelineOptionsMock.Object,
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

    [Fact]
    public async Task BadRequestPostDischargesExceptionNullResponse()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.AwaitingDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id);
      referral.Provider = _provider;
      referral.ReferralSource = null;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      string messageTemplate = "Post Discharge for referral {referralId} returned 400 Bad " +
        "Request with empty request body.";
      SetHttpMessageHttpHandlerMockPostBadRequest(null);

      // Act.
      Exception resultException = await Record
        .ExceptionAsync(() => _service.PostDischarges(discharges));

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<PostDischargesException>();
      _mockLogger.Verify(t => t.Error(messageTemplate, referral.Id), Times.Once);
    }

    [Fact]
    public async Task BadRequestPostDischargesExceptionNoErrors()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.AwaitingDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id);
      referral.Provider = _provider;
      referral.ReferralSource = null;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      string responseTitle = "One or more validation errors occurred.";
      GpDocumentProxyPostBadRequest expectedResponse = new()
      {
        Errors = new(),
        Title = responseTitle,
      };
      string messageTemplate = "Post Discharge for referral {referralId} returned 400 Bad " +
        "Request with error {Title}.";
      SetHttpMessageHttpHandlerMockPostBadRequest(expectedResponse);

      // Act.
      Exception resultException = await Record
        .ExceptionAsync(() => _service.PostDischarges(discharges));

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<PostDischargesException>();
      _mockLogger.Verify(t => t.Error(messageTemplate, referral.Id, responseTitle), Times.Once);
    }

    [Fact]
    public async Task BadRequestPostDischargesExceptionSingleError()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.AwaitingDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id);
      referral.FamilyName = null;
      referral.Provider = _provider;
      referral.ReferralSource = null;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      string responseTitle = "One or more validation errors occurred.";
      string error1 = "The FamilyName field is required.";
      string error2 = "The ReferralSource field is required.";
      GpDocumentProxyPostBadRequest expectedResponse = new()
      {
        Errors = new()
        {
          { "FamilyName", [error1] },
          { "ReferralSource", [error2] }
        },
        Title = responseTitle,
      };
      string messageTemplate = "Post Discharge for referral {referralId} returned 400 Bad " +
        "Request with error {Title}: {allErrors}.";
      SetHttpMessageHttpHandlerMockPostBadRequest(expectedResponse);

      // Act.
      Exception resultException = await Record
        .ExceptionAsync(() => _service.PostDischarges(discharges));

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<PostDischargesException>();
      _mockLogger.Verify(t =>
        t.Error(messageTemplate, referral.Id, responseTitle, $"{error1} {error2}"),
        Times.Once);
    }

    [Fact]
    public async Task NullTemplateIdPostDischargesException()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.AwaitingDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id,
       referralSource: ReferralSource.GpReferral);
      referral.Provider = _provider;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      discharge.TemplateId = null;
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      string messageTemplate = "Post Discharge for referral {ReferralId} has no Template Id " +
        "that matches its programme outcome.";

      // Act.
      Exception resultException = await Record
        .ExceptionAsync(() => _service.PostDischarges(discharges));

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<PostDischargesException>();
      _mockLogger.Verify(t => t.Error(messageTemplate, referral.Id), Times.Once);
    }

    [Fact]
    public async Task SystemErrorPostDischargesException()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.AwaitingDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id);
      referral.Provider = _provider;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      string exceptionMessage = "Test Exception";
      string errorMessage = $"{PostDischargesExceptionMessage}{exceptionMessage}";
      SetHttpMessageHttpHandlerMockPostException(exceptionMessage);

      // Act.
      Exception resultException = await Record
        .ExceptionAsync(() => _service.PostDischarges(discharges));

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<PostDischargesException>();
      resultException.Message.Should().Be(errorMessage);
      _mockLogger.Verify(t => t.Error(It.IsAny<Exception>(), exceptionMessage), Times.Once);
    }

    [Fact]
    public async Task UnauthorizedPostDischargesException()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.AwaitingDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id);
      referral.Provider = _provider;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      SetHttpMessageHttpHandlerMockPostNoContent(HttpStatusCode.Unauthorized);

      // Act.
      Exception resultException = await Record
        .ExceptionAsync(() => _service.PostDischarges(discharges));

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<InvalidTokenException>();
    }

    [Fact]
    public async Task InternalServerErrorPostDischargesException()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.AwaitingDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id);
      referral.Provider = _provider;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      string messageTemplate = "Post Discharge for referral {ReferralId} returned status " +
        "{StatusCode}.";
      HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
      SetHttpMessageHttpHandlerMockPostNoContent(statusCode);

      // Act.
      Exception resultException = await Record
        .ExceptionAsync(() => _service.PostDischarges(discharges));

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<PostDischargesException>();
      _mockLogger.Verify(t => t.Error(messageTemplate, referral.Id, statusCode), Times.Once);
    }

    [Theory]
    [MemberData(nameof(ProgrammeOutcomesTheoryData),
      new ProgrammeOutcome[] { ProgrammeOutcome.NotSet })]
    public async Task ValidProgrammeOutcomes(ProgrammeOutcome programmeOutcome)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.AwaitingDischarge,
       programmeOutcome: programmeOutcome.ToString(),
       providerId: _provider.Id,
       referralSource: ReferralSource.GpReferral);
      referral.Provider = _provider;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        GpDocumentProxyHelper.ProgrammeOutcomesRequiringMessage(),
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      GpDocumentProxyPostResponse response = new()
      {
        DocumentStatus = DocumentStatus.Received.ToString(),
        ReferralId = referral.Id
      };
      SetHttpMessageHttpHandlerMockPost(response);

      // Act.
      List<Guid> result = await _service.PostDischarges(discharges);

      // Assert.
      result.Should().NotBeNullOrEmpty();
      result.Should().HaveCount(1);
      result[0].Should().Be(referral.Id);
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData))]
    public async Task ValidReferralSources(ReferralSource referralSource)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.AwaitingDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id,
       referralSource: referralSource);
      referral.Provider = _provider;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      GpDocumentProxyPostResponse response = new()
      {
        DocumentStatus = DocumentStatus.Received.ToString(),
        ReferralId = referral.Id
      };
      SetHttpMessageHttpHandlerMockPost(response);

      // Act.
      List<Guid> result = await _service.PostDischarges(discharges);

      // Assert.
      result.Should().NotBeNullOrEmpty();
      result.Should().HaveCount(1);
      result[0].Should().Be(referral.Id);
    }

    [Fact]
    public async Task NullResponseBodyException()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.AwaitingDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id);
      referral.Provider = _provider;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      string messageTemplate = "Post Discharge for referral {referralId} returned 200 OK with " +
        "empty request body.";
      SetHttpMessageHttpHandlerMockPost(null);

      // Act.
      Exception resultException = await Record
        .ExceptionAsync(() => _service.PostDischarges(discharges));

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<PostDischargesException>();
      _mockLogger.Verify(t => t.Error(messageTemplate, referral.Id), Times.Once);
    }

    [Fact]
    public async Task NoMatchingReferralException()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.AwaitingDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id);
      referral.Provider = _provider;
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      string messageTemplate = "Post Discharge for referral {referralId} has no matching " +
        "referral.";
      GpDocumentProxyPostResponse response = new()
      {
        DocumentStatus = DocumentStatus.Received.ToString(),
        ReferralId = referral.Id
      };
      SetHttpMessageHttpHandlerMockPost(response);

      // Act.
      Exception resultException = await Record
        .ExceptionAsync(() => _service.PostDischarges(discharges));

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<PostDischargesException>();
      _mockLogger.Verify(t => t.Error(messageTemplate, referral.Id), Times.Once);
    }

    [Fact]
    public async Task OrganisationNotSupportedComplete()
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.AwaitingDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id);
      referral.Provider = _provider;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      string message = 
        $"Docman does not support organisation {referral.ReferringGpPracticeNumber}.";
      GpDocumentProxyPostResponse response = new()
      {
        DocumentStatus = DocumentStatus.OrganisationNotSupported.ToString(),
        Message = message,
        ReferralId = referral.Id
      };
      SetHttpMessageHttpHandlerMockPost(response);

      // Act.
      List<Guid> result = await _service.PostDischarges(discharges);

      // Assert.
      result.Should().NotBeNullOrEmpty();
      result.Should().HaveCount(1);
      result[0].Should().Be(referral.Id);
      Referral dischargedReferral = _context.Referrals.Single(r => r.Id == referral.Id);
      dischargedReferral.Status.Should().Be(ReferralStatus.Complete.ToString());
    }

    [Theory]
    [InlineData(DocumentStatus.OrganisationNotSupported)]
    [InlineData(DocumentStatus.Received)]
    public async Task MskReferralAlreadySent(DocumentStatus documentStatus)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.SentForDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id,
       referralSource: ReferralSource.Msk);
      referral.Provider = _provider;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      GpDocumentProxyPostResponse response = new()
      {
        DocumentStatus = documentStatus.ToString(),
        ReferralId = referral.Id
      };
      SetHttpMessageHttpHandlerMockPost(response);

      // Act.
      List<Guid> result = await _service.PostDischarges(discharges);

      // Assert.
      result.Should().NotBeNullOrEmpty();
      result.Should().HaveCount(1);
      result[0].Should().Be(referral.Id);
      Referral dischargedReferral = _context.Referrals.Single(r => r.Id == referral.Id);
      dischargedReferral.Status.Should().Be(ReferralStatus.SentForDischarge.ToString());
    }

    [Theory]
    [MemberData(nameof(ReferralSourceTheoryData), new[] { ReferralSource.Msk })]
    public async Task NonMskReferralAlreadySentException(ReferralSource referralSource)
    {
      // Arrange.
      Referral referral = RandomEntityCreator.CreateRandomReferral(
       status: ReferralStatus.SentForDischarge,
       programmeOutcome: ProgrammeOutcome.Complete.ToString(),
       providerId: _provider.Id,
       referralSource: referralSource);
      referral.Provider = _provider;
      AddAndDetatchReferral(referral);
      GpDocumentProxyReferralDischarge discharge = GetDischarge(
        DischargeDestination.Gp,
        _outcomesRequiringMessage,
        referral);
      List<GpDocumentProxyReferralDischarge> discharges = new() { discharge };
      string messageTemplate = "Post Discharge for referral {referralId} with status {Status} " + 
        "returned document status {DocumentStatus} and message {Message}.";
      GpDocumentProxyPostResponse expectedResponse = new()
      {
        DocumentStatus = DocumentStatus.DischargePending.ToString(),
        Message = "Test Message",
        ReferralId = referral.Id
      };
      string errorMessage = $"Post Discharge for referral {referral.Id} with status " +
        $"{referral.Status} returned document status {expectedResponse.DocumentStatus} and " + 
        $"message {expectedResponse.Message}.";
      string exceptionMessage = $"{PostDischargesExceptionMessage}{errorMessage}";
      SetHttpMessageHttpHandlerMockPost(expectedResponse);

      // Act.
      Exception resultException = await Record
        .ExceptionAsync(() => _service.PostDischarges(discharges));

      // Assert.
      resultException.Should().NotBeNull();
      resultException.Should().BeOfType<PostDischargesException>();
      resultException.Message.Should().Be(exceptionMessage);
      _mockLogger.Verify(t => 
        t.Error(messageTemplate,
          referral.Id,
          referral.Status,
          expectedResponse.DocumentStatus,
          expectedResponse.Message), Times.Once);
    }

    private void AddAndDetatchReferral(Referral referral)
    {
      _ = _context.Referrals.Add(referral);
      _ = _context.SaveChanges();
      _context.Entry(referral).State = EntityState.Detached;
    }

    private GpDocumentProxyReferralDischarge GetDischarge(
      DischargeDestination dischargeDestination,
      string[] outcomesRequiringMessage,
      Referral referral)
    {
      return new()
      {
        DateCompletedProgramme = referral.DateCompletedProgramme,
        DateOfBirth = referral.DateOfBirth.Value,
        DateOfReferral = referral.DateOfReferral.Value,
        FamilyName = referral.FamilyName,
        GivenName = referral.GivenName,
        Id = referral.Id,
        LastRecordedWeight = referral.LastRecordedWeight,
        LastRecordedWeightDate = referral.LastRecordedWeightDate,
        Message = outcomesRequiringMessage.Contains(referral.ProgrammeOutcome)
          ? referral.StatusReason
          : null,
        NhsNumber = referral.NhsNumber,
        ProviderName = referral.Provider.Name,
        ProgrammeOutcome = referral.ProgrammeOutcome,
        ReferringOrganisationOdsCode = dischargeDestination == DischargeDestination.Gp
          ? referral.ReferringGpPracticeNumber
          : referral.ReferringOrganisationOdsCode,
        ReferralSource = referral.ReferralSource,
        Sex = referral.Sex,
        TemplateId = dischargeDestination == DischargeDestination.Gp
          ? _gpDocumentProxyOptions.Gp.GetTemplateId(referral.ProgrammeOutcome)
          : _gpDocumentProxyOptions.Msk.GetTemplateId(referral.ProgrammeOutcome),
        Ubrn = referral.Ubrn,
        WeightOnReferral = referral.FirstRecordedWeight,
      };
    }

    private void SetHttpMessageHttpHandlerMockPost(
      GpDocumentProxyPostResponse data)
    {
      StringContent content = new(JsonConvert.SerializeObject(data));
      HttpStatusCode statusCode = HttpStatusCode.OK;
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath
          .Contains("/discharge")),
        ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          Content = content,
          StatusCode = statusCode
        });
    }

    private void SetHttpMessageHttpHandlerMockPostBadRequest(
      GpDocumentProxyPostBadRequest data)
    {
      StringContent content = new(JsonConvert.SerializeObject(data));
      HttpStatusCode statusCode = HttpStatusCode.BadRequest;
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath
          .Contains("/discharge")),
        ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          Content = content,
          StatusCode = statusCode
        });
    }

    private void SetHttpMessageHttpHandlerMockPostNoContent(HttpStatusCode statusCode)
    {
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath
          .Contains("/discharge")),
        ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
          StatusCode = statusCode
        });
    }

    private void SetHttpMessageHttpHandlerMockPostException(string message)
    {
      _httpMessageHandlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.AbsolutePath
          .Contains("/discharge")),
        ItExpr.IsAny<CancellationToken>())
        .ThrowsAsync(new UnitTestExpectedExceptionNotThrownException(message));
    }
  }
}