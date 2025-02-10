using FluentAssertions;
using MELT;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Mime;
using WmsHub.AzureFunctions.Exceptions;
using WmsHub.AzureFunctions.Options;
using WmsHub.AzureFunctions.Services;

namespace WmsHub.AzureFunctions.Tests.Services;
public class SendTextMessagesServiceTests : IDisposable
{
  private readonly MockHttpMessageHandler _mockHttpMessageHandler;
  private readonly SendTextMessagesOptions _sendTextMessagesOptions;
  private readonly SendTextMessagesService _sendTextMessagesService;
  private readonly ITestLoggerFactory _testLoggerFactory;

  public SendTextMessagesServiceTests()
  {
    _mockHttpMessageHandler = new MockHttpMessageHandler();
    _sendTextMessagesOptions = new SendTextMessagesOptions
    {
      ApiKey = "api-key",
      BatchSize = 2,
      MaxSendRetries = 1,
      TextMessageApiUrl = "https://test.com"
    };
    _testLoggerFactory = TestLoggerFactory.Create();

    _sendTextMessagesService = new SendTextMessagesService(
      _mockHttpMessageHandler.ToHttpClient(),
      _testLoggerFactory,
      Microsoft.Extensions.Options.Options.Create(_sendTextMessagesOptions));
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    _mockHttpMessageHandler.Dispose();
  }

  public class ProcessAsyncTests : SendTextMessagesServiceTests
  {
    [Fact]
    public async Task Should_ProcessSuccessfullyOneTextSent()
    {
      string checkSent = "1";
      string prepared = "1";
      string sent = "1";
      // Arrange.
      string expectedResult = $"Prepared: {prepared}, Checked: {checkSent}, Sent: {sent}";

      _mockHttpMessageHandler
        .Expect(HttpMethod.Get, _sendTextMessagesOptions.PrepareUrl)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Text.Plain, prepared);
      _mockHttpMessageHandler
        .Expect(HttpMethod.Get, _sendTextMessagesOptions.CheckSendUrl)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Text.Plain, checkSent);
      _mockHttpMessageHandler
        .Expect(HttpMethod.Get, _sendTextMessagesOptions.SendUrl)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Text.Plain, sent);

      // Act.
      string result = await _sendTextMessagesService.ProcessAsync();

      // Assert.
      result.Should().Be(expectedResult);
      _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Should_ProcessSuccessfullyZeroTextsSent()
    {
      string numberOfTexts = "0";
      // Arrange.
      string expectedResult = 
        $"Prepared: {numberOfTexts}, Checked: {numberOfTexts}, Sent: {numberOfTexts}";

      _mockHttpMessageHandler
        .Expect(HttpMethod.Get, _sendTextMessagesOptions.PrepareUrl)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Text.Plain, numberOfTexts);

      // Act.
      string result = await _sendTextMessagesService.ProcessAsync();

      // Assert.
      result.Should().Be(expectedResult);
      _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Should_Throw_When_FewerMessagesSentThanChecked()
    {
      // Arrange.
      string expectedMessage = $"Checked: 1, Sent: 0";

      _mockHttpMessageHandler
        .Expect(HttpMethod.Get, _sendTextMessagesOptions.PrepareUrl)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Text.Plain, "1");
      _mockHttpMessageHandler
        .Expect(HttpMethod.Get, _sendTextMessagesOptions.CheckSendUrl)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Text.Plain, "1");
      _mockHttpMessageHandler
        .Expect(HttpMethod.Get, _sendTextMessagesOptions.SendUrl)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Text.Plain, "0");

      // Act.
      Func<Task> act = _sendTextMessagesService.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<SendTextMessagesException>().WithMessage(expectedMessage);
      _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Should_Throw_When_RequestReturnsNot200OkStatusCode()
    {
      // Arrange.
      string expectedMessage = $"GET request to '{_sendTextMessagesOptions.PrepareUrl}' returned " +
        $"a status code of {HttpStatusCode.InternalServerError:d}.";
      _mockHttpMessageHandler
        .Expect(HttpMethod.Get, _sendTextMessagesOptions.PrepareUrl)
        .Respond(HttpStatusCode.InternalServerError);

      // Act.
      Func<Task> act = _sendTextMessagesService.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<HttpRequestException>().WithMessage(expectedMessage);
      _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Should_Throw_When_RequestReturnsNotAnInteger()
    {
      // Arrange.
      string responseString = "test";
      string expectedMessage = $"GET request to '{_sendTextMessagesOptions.PrepareUrl}' returned " +
        $"non-integer content: {responseString}.";
      _mockHttpMessageHandler
        .Expect(HttpMethod.Get, _sendTextMessagesOptions.PrepareUrl)
        .Respond(HttpStatusCode.OK, MediaTypeNames.Text.Plain, responseString);

      // Act.
      Func<Task> act = _sendTextMessagesService.ProcessAsync;

      // Assert.
      await act.Should().ThrowAsync<FormatException>().WithMessage(expectedMessage);
      _mockHttpMessageHandler.VerifyNoOutstandingExpectation();
    }
  }
}
