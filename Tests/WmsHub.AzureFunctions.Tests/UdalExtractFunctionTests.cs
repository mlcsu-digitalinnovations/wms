using FluentAssertions;
using MELT;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Moq;
using WmsHub.AzureFunctions.Services;

namespace WmsHub.AzureFunctions.Tests;

public class UdalExtractFunctionTests
{
  public class RunTests : UdalExtractFunctionTests
  {
    [Fact]
    public async Task Should_Invoke_RunBaseAsyncWithAppNameSet()
    {
      // Arrange.
      ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
      Mock<IProcessStatusService> mockProcessStatusService = new();
      Mock<IUdalExtractService> udalExtractService = new();
      TestUdalExtractFunction testUdalExtractFunction = new(
        loggerFactory,
        mockProcessStatusService.Object,
        udalExtractService.Object);
      string expectedAppname = "WmsHub.AzureFunctions.UdalExtract";

      TimerInfo timerInfo = new();

      // Act.
      await testUdalExtractFunction.Run(timerInfo);

      // Assert.
      mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      mockProcessStatusService.VerifySet(p => p.AppName = expectedAppname, Times.Once());
    }
  }

  public class RunProcessAsyncTests : UdalExtractFunctionTests
  {
    [Fact]
    public async Task Should_Invoke_SqlMaintenanceService_ProcessAsync()
    {
      // Arrange.
      ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
      Mock<IProcessStatusService> mockProcessStatusService = new();
      Mock<IUdalExtractService> udalExtractService = new();
      string expectedProcessResult = "Process completed successfully";

      udalExtractService
        .Setup(s => s.ProcessAsync())
        .ReturnsAsync(expectedProcessResult);

      TestUdalExtractFunction testUdalExtractFunction = new(
        loggerFactory,
        mockProcessStatusService.Object,
        udalExtractService.Object);

      // Act.
      string actualResult = await testUdalExtractFunction.RunProcessAsync();

      // Assert.
      actualResult.Should().Be(expectedProcessResult);
      udalExtractService.Verify(s => s.ProcessAsync(), Times.Once);
    }
  }

  // Derived class for exposing protected methods
  private class TestUdalExtractFunction(
    ILoggerFactory loggerFactory,
    IProcessStatusService processStatusService,
    IUdalExtractService udalExtractService)
      : UdalExtractFunction(loggerFactory, processStatusService, udalExtractService)
  {
    public new async Task<string> RunProcessAsync()
    {
      return await base.RunProcessAsync();
    }
  }
}

