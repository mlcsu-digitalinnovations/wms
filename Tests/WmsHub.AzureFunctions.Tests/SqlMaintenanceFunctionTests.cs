using FluentAssertions;
using MELT;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Moq;
using WmsHub.AzureFunctions.Services;

namespace WmsHub.AzureFunctions.Tests;

public class SqlMaintenanceFunctionTests
{
  public class RunTests : SqlMaintenanceFunctionTests
  {
    [Fact]
    public async Task Should_Invoke_RunBaseAsyncWithAppNameSet()
    {
      // Arrange.
      ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
      Mock<IProcessStatusService> mockProcessStatusService = new();
      Mock<ISqlMaintenanceService> mockSqlMaintenanceService = new();
      TestSqlMaintenanceFunction testSqlMaintenanceFunction = new(
        loggerFactory,
        mockProcessStatusService.Object,
        mockSqlMaintenanceService.Object);
      string expectedAppname = "WmsHub.AzureFunctions.SqlMaintenance";

      TimerInfo timerInfo = new();

      // Act.
      await testSqlMaintenanceFunction.Run(timerInfo);

      // Assert.
      mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once());
      mockProcessStatusService.VerifySet(p => p.AppName = expectedAppname, Times.Once());
    }
  }

  public class RunProcessAsyncTests : SqlMaintenanceFunctionTests
  {
    [Fact]
    public async Task Should_Invoke_SqlMaintenanceService_ProcessAsync()
    {
      // Arrange.
      ITestLoggerFactory loggerFactory = TestLoggerFactory.Create();
      Mock<IProcessStatusService> mockProcessStatusService = new();
      Mock<ISqlMaintenanceService> mockSqlMaintenanceService = new();
      string expectedProcessResult = "Process completed successfully";

      mockSqlMaintenanceService
        .Setup(s => s.ProcessAsync())
        .ReturnsAsync(expectedProcessResult);

      TestSqlMaintenanceFunction testSqlMaintenanceFunction = new(
        loggerFactory,
        mockProcessStatusService.Object,
        mockSqlMaintenanceService.Object);

      // Act.
      string actualResult = await testSqlMaintenanceFunction.RunProcessAsync();

      // Assert.
      actualResult.Should().Be(expectedProcessResult);
      mockSqlMaintenanceService.Verify(s => s.ProcessAsync(), Times.Once);
    }
  }

  // Derived class for exposing protected methods
  private class TestSqlMaintenanceFunction(
    ILoggerFactory loggerFactory,
    IProcessStatusService processStatusService,
    ISqlMaintenanceService sqlMaintenanceService)
      : SqlMaintenanceFunction(loggerFactory, processStatusService, sqlMaintenanceService)
  {
    public new async Task<string> RunProcessAsync() => await base.RunProcessAsync();
  }
}
