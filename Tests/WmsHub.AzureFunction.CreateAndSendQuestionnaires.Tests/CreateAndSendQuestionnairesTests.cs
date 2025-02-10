using FluentAssertions;
using Microsoft.Extensions.Options;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Mime;
using WmsHub.Tests.Helper;
using Xunit;

namespace WmsHub.AzureFunction.CreateAndSendQuestionnaires.Tests;
public class CreateAndSendQuestionnairesTests : CreateAndSendQuestionnairesTestData, IDisposable
{
  private readonly CreateAndSendQuestionnaires _createAndSendQuestionnaires;
  private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
  private readonly MockHttpMessageHandler _mockHttpMessageHandler;
  private readonly SerilogLoggerMock _mockLogger;
  private readonly Mock<IProcessStatusService> _mockProcessStatusService;
  private readonly IOptions<CreateAndSendQuestionnairesOptions> _options;

  public CreateAndSendQuestionnairesTests()
  {
    _options = Options.Create(new CreateAndSendQuestionnairesOptions()
    {
      CreateQuestionnairesPath = TestCreateQuestionnairesPath,
      MaximumIterations = TestMaximumIterations,
      ReferralApiBaseUrl = TestBaseUrl,
      ReferralApiQuestionnaireKey = TestReferralApiQuestionnaireKey,
      SendQuestionnairesPath = TestSendQuestionnairesPath
    });

    _mockHttpClientFactory = new Mock<IHttpClientFactory>();
    _mockHttpMessageHandler = new();
    _mockLogger = new();
    _mockProcessStatusService = new();

    _createAndSendQuestionnaires = new(
      _mockHttpClientFactory.Object,
      _mockLogger,
      _options,
      _mockProcessStatusService.Object);
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    _mockHttpMessageHandler?.Dispose();
  }

  public class Constructor : CreateAndSendQuestionnairesTests
  {
    [Fact]
    public void Constructor_ShouldInitializeFields_WhenValidArgumentsAreProvided()
    {
      // Arrange.

      // Act.
      CreateAndSendQuestionnaires createAndSendQuestionnaires = new(
        _mockHttpClientFactory.Object,
        _mockLogger,
        _options,
        _mockProcessStatusService.Object);

      // Assert.
      createAndSendQuestionnaires.Should().NotBeNull();
      createAndSendQuestionnaires.Should().BeOfType<CreateAndSendQuestionnaires>();
    }
  }

  public class RunTests : CreateAndSendQuestionnairesTests
  {
    [Fact]
    public async Task Run_WhenCreateResponseIsConflict_ShouldSendFailureMessage()
    {
      // Arrange.
      string expectedError = $"{CreateQuestionnairesErrorMessage}'{HttpStatusCode.Conflict}': ''.";

      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond(HttpStatusCode.Conflict);

      MockedRequest sendQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestSendQuestionnairesPath}")
        .Respond(HttpStatusCode.NoContent);

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer());

      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);
      int sendQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        sendQuestionnairesRequest);

      // Assert.
      _mockLogger.Messages.Should().Contain(expectedError);

      createQuestionnairesHttpCalls.Should().Be(1);
      sendQuestionnairesHttpCalls.Should().Be(0);

      _mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      _mockProcessStatusService.Verify(p => p.FailureAsync(expectedError), Times.Once());
    }

    [Fact]
    public async Task Run_WhenCreateResponseIsEmptyResponse_ShouldSendFailureMessage()
    {
      // Arrange.
      string expectedError =
        $"{CreateQuestionnairesErrorMessage}'{HttpStatusCode.BadRequest}': ''.";

      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, "");

      MockedRequest sendQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestSendQuestionnairesPath}")
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, "");

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer());

      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);
      int sendQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        sendQuestionnairesRequest);

      // Assert.
      _mockLogger.Exceptions.Should().ContainSingle();

      createQuestionnairesHttpCalls.Should().Be(1);
      sendQuestionnairesHttpCalls.Should().Be(0);

      _mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      _mockProcessStatusService.Verify(p => p.FailureAsync(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task Run_WhenCreateResponseIsErrorCode_ShouldSendFailureMessage()
    {
      // Arrange.
      string expectedError =
        $"{CreateQuestionnairesErrorMessage}'{HttpStatusCode.InternalServerError}': ''.";

      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond(HttpStatusCode.InternalServerError);

      MockedRequest sendQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestSendQuestionnairesPath}")
        .Respond(HttpStatusCode.NoContent);

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer());

      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);
      int sendQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        sendQuestionnairesRequest);

      // Assert.
      _mockLogger.Messages.Should().Contain(expectedError);

      createQuestionnairesHttpCalls.Should().Be(1);
      sendQuestionnairesHttpCalls.Should().Be(0);

      _mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      _mockProcessStatusService.Verify(p => p.FailureAsync(expectedError), Times.Once());
    }

    [Fact]
    public async Task Run_WhenCreateResponseIsNotCorrectType_ShouldSendFailureMessage()
    {
      // Arrange.
      string createResponse = @"{""Test"":""Test""}";
      string expectedError =
        $"{CreateQuestionnairesErrorMessage}'{HttpStatusCode.BadRequest}': ''.";

      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, createResponse);

      MockedRequest sendQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestSendQuestionnairesPath}")
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, createResponse);

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer());

      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);
      int sendQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        sendQuestionnairesRequest);

      // Assert.
      _mockLogger.Exceptions.Should().ContainSingle();
      _mockLogger.Messages.Should().ContainSingle();

      createQuestionnairesHttpCalls.Should().Be(1);
      sendQuestionnairesHttpCalls.Should().Be(0);

      _mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      _mockProcessStatusService.Verify(p => p.FailureAsync(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task Run_WhenCreateResponseIsNotSuccessStatusCode_ShouldSendFailureMessage()
    {
      // Arrange.
      string expectedError =
        $"{CreateQuestionnairesErrorMessage}'{HttpStatusCode.BadRequest}': ''.";

      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond(HttpStatusCode.BadRequest);

      MockedRequest sendQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestSendQuestionnairesPath}")
        .Respond(HttpStatusCode.NoContent);

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer());

      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);
      int sendQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        sendQuestionnairesRequest);

      // Assert.
      _mockLogger.Messages.Should().Contain(expectedError);

      createQuestionnairesHttpCalls.Should().Be(1);
      sendQuestionnairesHttpCalls.Should().Be(0);

      _mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      _mockProcessStatusService.Verify(p => p.FailureAsync(expectedError), Times.Once());
    }

    [Fact]
    public async Task Run_WhenCreateResponseReturnsSuccessWithErrors_ShouldSendFailureMessage()
    {
      // Arrange.
      const int numberOfQuestionnaires = 5;
      const int numberOfErrors = 1;
      string expectedError =
        $"{CreateQuestionnairesErrorMessage}{numberOfErrors} errors: '{ErrorText}'.";

      string createResponse = ConvertToString(
        GetCreateQuestionnaireResponse(numberOfQuestionnaires, numberOfErrors));

      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, createResponse);

      MockedRequest sendQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestSendQuestionnairesPath}")
        .Respond(HttpStatusCode.NoContent);

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer());

      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);
      int sendQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        sendQuestionnairesRequest);

      // Assert.
      _mockLogger.Messages.Should()
        .Contain($"Number of questionnaires created: {numberOfQuestionnaires}.")
        .And.Contain(expectedError);

      createQuestionnairesHttpCalls.Should().Be(1);
      sendQuestionnairesHttpCalls.Should().Be(0);

      _mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      _mockProcessStatusService.Verify(p => p.FailureAsync(expectedError), Times.Once());
    }

    [Fact]
    public async Task Run_WhenIsPastDue_ShouldLogFunctionRunningLate()
    {
      // Arrange.
      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond(HttpStatusCode.NoContent);

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer { IsPastDue = true });

      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);

      // Assert.
      _mockLogger.Messages.Should()
        .Contain($"{nameof(CreateAndSendQuestionnaires)} Azure Function executed late.");

      createQuestionnairesHttpCalls.Should().Be(1);
    }

    [Fact]
    public async Task Run_WhenOverMaximumIterations_ShouldSendFailureMessage()
    {
      // Arrange.
      const int totalNumberOfQuestionnaires = 1500;
      const int expectedIterations = 4;
      int createCount = 0;
      int sendCount = 0;
      int expectedSent = expectedIterations * MaximumQuestionnaires;

      string expectedLog = $"Sent questionnaires: {expectedSent}. Failed " +
        $"questionnaires: 0. Iterations: {expectedIterations}";
      string expectedStatusMessage =
        $"Exceeded max iterations of '{TestMaximumIterations}'. {expectedLog}";

      List<HttpResponseMessage> createResponses =
        GetCreateQuestionnaireResponseList(totalNumberOfQuestionnaires);
      List<HttpResponseMessage> sendResponses =
        GetSendQuestionnaireResponseList(totalNumberOfQuestionnaires);

      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond((request) => createResponses[createCount++]);

      MockedRequest sendQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestSendQuestionnairesPath}")
        .Respond((request) => sendResponses[sendCount++]);

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer());
      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);
      int sendQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        sendQuestionnairesRequest);

      // Assert.
      _mockLogger.Messages.Should().Contain(expectedLog);

      createQuestionnairesHttpCalls.Should().Be(expectedIterations);
      sendQuestionnairesHttpCalls.Should().Be(expectedIterations);

      _mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      _mockProcessStatusService.Verify(p => p.FailureAsync(expectedStatusMessage), Times.Once());
    }

    [Fact]
    public async Task Run_WhenOverMaxNumberToCreate_ShouldMakeMultipleCallsToCreateAndSend()
    {
      // Arrange.
      const int totalQuestionnaires = 300;
      const int expectedIterations = 3;
      int createCount = 0;
      int sendCount = 0;
      string expectedMessage = $"Sent questionnaires: {totalQuestionnaires}. Failed " +
        $"questionnaires: 0. Iterations: {expectedIterations}";

      List<HttpResponseMessage> createResponses =
        GetCreateQuestionnaireResponseList(totalQuestionnaires);
      List<HttpResponseMessage> sendResponses =
        GetSendQuestionnaireResponseList(totalQuestionnaires);

      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond((request) => createResponses[createCount++]);

      MockedRequest sendQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestSendQuestionnairesPath}")
        .Respond((request) => sendResponses[sendCount++]);

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer());

      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);
      int sendQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        sendQuestionnairesRequest);

      // Assert.
      _mockLogger.Messages.Should().Contain(expectedMessage);

      createQuestionnairesHttpCalls.Should().Be(expectedIterations);
      sendQuestionnairesHttpCalls.Should().Be(expectedIterations);

      _mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      _mockProcessStatusService.Verify(p => p.SuccessAsync(expectedMessage), Times.Once());
    }

    [Fact]
    public async Task Run_WhenSendResponseIsNoContentStatusCode_ShouldLogZeroToSend()
    {
      // Arrange.
      string expectedError = "Zero questionnaires to send.";

      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond(HttpStatusCode.NoContent);

      MockedRequest sendQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestSendQuestionnairesPath}")
        .Respond(HttpStatusCode.NoContent);

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer());

      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);
      int sendQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        sendQuestionnairesRequest);

      // Assert.
      _mockLogger.Messages.Should().Contain(expectedError);

      createQuestionnairesHttpCalls.Should().Be(1);
      sendQuestionnairesHttpCalls.Should().Be(1);

      _mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      _mockProcessStatusService.Verify(p => p.SuccessAsync(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task Run_WhenSendResponseIsNotSendQuestionnaireResponse_ShouldSendFailureMessage()
    {
      // Arrange.
      string createResponse = ConvertToString(GetCreateQuestionnaireResponse(1, 0));
      string sendResponse = @"{""Test"":""Test""}";
      string expectedError =
        $"{CreateQuestionnairesErrorMessage}'{HttpStatusCode.BadRequest}': ''.";

      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, createResponse);

      MockedRequest sendQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestSendQuestionnairesPath}")
        .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, sendResponse);

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer());

      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);
      int sendQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        sendQuestionnairesRequest);

      // Assert.
      _mockLogger.Exceptions.Should().ContainSingle();

      createQuestionnairesHttpCalls.Should().Be(1);
      sendQuestionnairesHttpCalls.Should().Be(1);

      _mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      _mockProcessStatusService.Verify(p => p.FailureAsync(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task Run_WhenSendResponseIsNotSuccessStatusCode_ShouldSendFailureMessage()
    {
      // Arrange.
      string expectedError = $"{SendQuestionnairesErrorMessage}'{HttpStatusCode.BadRequest}': ''.";

      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond(HttpStatusCode.NoContent);

      MockedRequest sendQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestSendQuestionnairesPath}")
        .Respond(HttpStatusCode.BadRequest);

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer());

      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);
      int sendQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        sendQuestionnairesRequest);

      // Assert.
      _mockLogger.Messages.Should().Contain(expectedError);

      createQuestionnairesHttpCalls.Should().Be(1);
      sendQuestionnairesHttpCalls.Should().Be(1);

      _mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      _mockProcessStatusService.Verify(p => p.FailureAsync(expectedError), Times.Once());
    }

    [Fact]
    public async Task Run_WhenSingleQuestionnaire_ShouldCallCreateAndSendTwice()
    {
      // Arrange.
      const int numberOfQuestionnaires = 1;
      const int expectedIterations = 2;
      int createCount = 0;
      int sendCount = 0;
      string expectedMessage = $"Sent questionnaires: {numberOfQuestionnaires}. Failed " +
        $"questionnaires: 0. Iterations: {expectedIterations}";

      List<HttpResponseMessage> createResponses =
        GetCreateQuestionnaireResponseList(numberOfQuestionnaires);
      List<HttpResponseMessage> sendResponses =
        GetSendQuestionnaireResponseList(numberOfQuestionnaires);

      MockedRequest createQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestCreateQuestionnairesPath}")
        .Respond((request) => createResponses[createCount++]);

      MockedRequest sendQuestionnairesRequest = _mockHttpMessageHandler
        .When(HttpMethod.Post, $"{TestBaseUrl}{TestSendQuestionnairesPath}")
        .Respond((request) => sendResponses[sendCount++]);

      ConfigureMockHttpClient();

      // Act.
      await _createAndSendQuestionnaires.Run(new Models.Timer());

      int createQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        createQuestionnairesRequest);
      int sendQuestionnairesHttpCalls = _mockHttpMessageHandler.GetMatchCount(
        sendQuestionnairesRequest);

      // Assert.
      _mockLogger.Messages.Should().Contain(expectedMessage);

      createQuestionnairesHttpCalls.Should().Be(expectedIterations);
      sendQuestionnairesHttpCalls.Should().Be(expectedIterations);

      _mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      _mockProcessStatusService.Verify(p => p.SuccessAsync(expectedMessage), Times.Once());
    }

    private void ConfigureMockHttpClient()
    {
      HttpClient httpClientMock = new(_mockHttpMessageHandler);
      _mockHttpClientFactory.Setup(c => c.CreateClient(It.IsAny<string>())).Returns(httpClientMock);
    }
  }
}