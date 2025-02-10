using FluentAssertions;
using MELT;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Moq;
using WmsHub.AzureFunctions.Options;
using WmsHub.AzureFunctions.Services;

namespace WmsHub.AzureFunctions.Tests;
public class SendTextMessagesFunctionTests
{
  public class RunTests : SendTextMessagesFunctionTests
  {
    [Fact]
    public async Task Should_Invoke_RunBaseAsyncWithAppNameSet()
    {
      // Arrange.
      string expectedAppname = "WmsHub.AzureFunctions.SendTextMessages.Daily";
      SendTextMessagesOptions sendTextMessagesOptions = new()
      {
        ApiKey = "api-key",
        TextMessageApiUrl = "https://test.com"
      };
      ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
      Mock<IProcessStatusService> mockProcessStatusService = new();
      Mock<IOptions<SendTextMessagesOptions>> mockSendTextMessagesOptions = new();
      mockSendTextMessagesOptions.Setup(x => x.Value).Returns(sendTextMessagesOptions);
      Mock<ISendTextMessagesService> mockSendTextMessagesService = new();
      TestSendTextMessagesFunction testSendTextMessagesFunction = new(
        loggerFactory,
        mockProcessStatusService.Object,
        mockSendTextMessagesOptions.Object,
        mockSendTextMessagesService.Object);

      TimerInfo timerInfo = new();

      // Act.
      await testSendTextMessagesFunction.Run(timerInfo);

      // Assert.
      mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      mockProcessStatusService.VerifySet(p => p.AppName = expectedAppname, Times.Once());
    }
  }

  public class RunProcessTests : SendTextMessagesFunctionTests
  {
    [Fact]
    public async Task Should_Invoke_SqlMaintenanceService_ProcessAsync()
    {
      // Arrange.
      ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
      Mock<IOptions<SendTextMessagesOptions>> mockSendTextMessagesOptions = new();
      Mock<IProcessStatusService> mockProcessStatusService = new();
      Mock<ISendTextMessagesService> mockSendTextMessagesService = new();
      string expectedProcessResult = "Process completed successfully";

      mockSendTextMessagesService
        .Setup(s => s.ProcessAsync())
        .ReturnsAsync(expectedProcessResult);

      TestSendTextMessagesFunction testSendTextMessagesFunction = new(
        loggerFactory,
        mockProcessStatusService.Object,
        mockSendTextMessagesOptions.Object,
        mockSendTextMessagesService.Object);

      // Act.
      string actualResult = await testSendTextMessagesFunction.RunProcessAsync();

      // Assert.
      actualResult.Should().Be(expectedProcessResult);
      mockSendTextMessagesService.Verify(s => s.ProcessAsync(), Times.Once);
    }
  }

  // Derived class for exposing protected methods
  private class TestSendTextMessagesFunction(
    ILoggerFactory loggerFactory,
    IProcessStatusService processStatusService,
    IOptions<SendTextMessagesOptions> sendTextMessagesOptions,
    ISendTextMessagesService sendTextMessagesService)
      : SendTextMessagesFunction(
        loggerFactory,
        processStatusService,
        sendTextMessagesOptions,
        sendTextMessagesService)
  {
    public new async Task<string> RunProcessAsync()
    {
      return await base.RunProcessAsync();
    }
  }
}
