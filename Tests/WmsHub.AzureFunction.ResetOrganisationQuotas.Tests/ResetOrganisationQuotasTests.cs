using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace WmsHub.AzureFunction.ResetOrganisationQuotas.Tests;
public class ResetOrganisationQuotasTests
{
  protected ResetOrganisationQuotas _testClass;

  protected HttpClient _httpClient;
  protected Mock<IHttpClientFactory> _mockHttpClientFactory;
  protected Mock<HttpMessageHandler> _mockHttpMessageHandler;
  protected Mock<ILogger<ResetOrganisationQuotas>> _mockLogger;
  protected Mock<ILoggerFactory> _mockLoggerFactory;
  protected IOptions<ResetOrganisationQuotasOptions> _options;

  protected const string RESET_URL = "https://ResetUrl.test";

  public ResetOrganisationQuotasTests()
  {
    _options = Options.Create(
      new ResetOrganisationQuotasOptions()
      {
        ReferralApiAdminKey = "ApiKey",
        ResetOrganisationQuotasUrl = RESET_URL
      });

    _mockHttpMessageHandler = new();
    _mockHttpClientFactory = new Mock<IHttpClientFactory>();
    _httpClient = new(_mockHttpMessageHandler.Object);
    _mockHttpClientFactory
      .Setup(hcf => hcf.CreateClient(string.Empty))
      .Returns(_httpClient);

    _mockLogger = new();
    _mockLoggerFactory = new Mock<ILoggerFactory>();
    _mockLoggerFactory.Setup(lf => lf.CreateLogger(typeof(ResetOrganisationQuotas).ToString()))
      .Returns(_mockLogger.Object);

    _testClass = new(_mockHttpClientFactory.Object, _mockLoggerFactory.Object, _options);
  }

  public class RunTests : ResetOrganisationQuotasTests
  {
    public RunTests() : base() { }

    [Fact]
    public async Task SuccessResponseCode_CallsEndpointAndCreatesLogs()
    {
      // Arrange.
      string expectedLogMessage = $"{RESET_URL} OK. Review logs for details.";

      HttpResponseMessage response = new(System.Net.HttpStatusCode.OK);

      AddMockHttpMessageHandlerSetupSendAsync(HttpMethod.Post, RESET_URL, response);

      // Act.
      await _testClass.Run(new Timer());

      // Assert.
      using (new AssertionScope())
      {
        VerifyHttpRequest(HttpMethod.Post, RESET_URL, Times.Once());
        VerifyLog(LogLevel.Information, expectedLogMessage, Times.Once());
      }
    }

    [Fact]
    public async Task ErrorResponseCode_CallsEndpointAndCreatesLogs()
    {
      // Arrange.
      string expectedLogMessage = $"{RESET_URL} returned unexpected result InternalServerError. " +
        $"Review logs for details.";

      HttpResponseMessage response = new(System.Net.HttpStatusCode.InternalServerError);

      AddMockHttpMessageHandlerSetupSendAsync(HttpMethod.Post, RESET_URL, response);

      // Act.
      await _testClass.Run(new Timer());

      // Assert.
      using (new AssertionScope())
      {
        VerifyHttpRequest(HttpMethod.Post, RESET_URL, Times.Once());
        VerifyLog(LogLevel.Error, expectedLogMessage, Times.Once());
      }
    }
  }

  private void AddMockHttpMessageHandlerSetupSendAsync(
    HttpMethod method,
    string uri,
    HttpResponseMessage response)
  {
    _mockHttpMessageHandler
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.Is<HttpRequestMessage>(req => 
            req.Method == method && req.RequestUri == new Uri(uri, UriKind.RelativeOrAbsolute)),
          ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(response);
  }

  private void VerifyHttpRequest(HttpMethod method, string uri, Times times)
  {
    _mockHttpMessageHandler.Protected().Verify(
          "SendAsync",
          times,
          ItExpr.Is<HttpRequestMessage>(req => 
            req.Method == method && req.RequestUri == new Uri(uri, UriKind.RelativeOrAbsolute)),
          ItExpr.IsAny<CancellationToken>());
  }

  private void VerifyLog(LogLevel level, string message, Times times)
  {
    _mockLogger.Verify(logger => logger.Log(
        level,
        It.Is<EventId>(eventId => eventId.Id == 0),
        It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == message),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);
  }
}
