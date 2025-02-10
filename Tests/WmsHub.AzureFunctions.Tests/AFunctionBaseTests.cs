using FluentAssertions;
using MELT;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Moq;

namespace WmsHub.AzureFunctions.Tests;

public class AFunctionBaseTests
{
  private const string RunProcessAsyncSuccessMessage = "Process Completed";
  private const string TestFunctionName = "TestFunction";
  private const string TestExceptionMessage = "Test exception";
  private static readonly InvalidOperationException s_testException = new(TestExceptionMessage);

  // A test implementation of AFunctionBase<T> for testing purposes
  private class TestFunction(
    ILoggerFactory loggerFactory,
    IProcessStatusService processStatusService)
      : AFunctionBase<TestFunction>(loggerFactory, processStatusService)
  {
    public new async Task RunBaseAsync(TimerInfo timerInfo) => await base.RunBaseAsync(timerInfo);

    protected override string FunctionName => TestFunctionName;

    protected override Task<string> RunProcessAsync() 
      => Task.FromResult(RunProcessAsyncSuccessMessage);
  }

  // A test implementation of AFunctionBase<T> for testing purposes that throws a 
  // InvalidOperationException when RunProcessAsync is called from RunBaseAsync.
  private class TestFunctionThrowException(
    ILoggerFactory loggerFactory,
    IProcessStatusService processStatusService)
      : TestFunction(loggerFactory, processStatusService)
  {
    protected override Task<string> RunProcessAsync() => throw s_testException;
  }

  // The following tests do not test for any debug logs.
  public class RunBaseAsyncTests : AFunctionBaseTests
  {
    [Fact]
    public async Task Should_SetAppNameAndNotifySuccess_When_RunProcessAsyncIsSuccessful()
    {
      // Arrange.
      ITestLoggerFactory testLoggerFactory = TestLoggerFactory.Create();

      Mock<IProcessStatusService> mockProcessStatusService = new();
      TestFunction testFunction = new(testLoggerFactory, mockProcessStatusService.Object);
      TimerInfo timerInfo = new();

      // Act.
      await testFunction.RunBaseAsync(timerInfo);

      // Assert.
      mockProcessStatusService.Verify(p => p.FailureAsync(It.IsAny<string>()), Times.Never);
      mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once);
      mockProcessStatusService.Verify(p => p.SuccessAsync(RunProcessAsyncSuccessMessage), Times.Once);
      mockProcessStatusService.VerifySet(p => p.AppName = TestFunctionName, Times.Once);
    }

    [Fact]
    public async Task Should_SetAppNameNotifyFailure_When_RunProcessThrowsException()
    {
      // Arrange.
      ITestLoggerFactory testLoggerFactory = TestLoggerFactory.Create();

      Mock<IProcessStatusService> mockProcessStatusService = new();
      TestFunctionThrowException function = new(testLoggerFactory, mockProcessStatusService.Object);
      TimerInfo timerInfo = new();

      // Act.
      await function.RunBaseAsync(timerInfo);

      // Assert.
      mockProcessStatusService.Verify(p => p.FailureAsync(TestExceptionMessage), Times.Once);
      mockProcessStatusService.Verify(p => p.StartedAsync(), Times.Once);
      mockProcessStatusService.Verify(p => p.SuccessAsync(It.IsAny<string>()), Times.Never);
      mockProcessStatusService.VerifySet(p => p.AppName = TestFunctionName, Times.Once);
      testLoggerFactory.Sink.LogEntries.Single(x => x.LogLevel == LogLevel.Error)
        .Exception.Should().NotBeNull()
        .And.Subject.Should().Be(s_testException);
    }

    [Fact]
    public async Task Should_LogTimerSchedule_When_TimerInfoHasScheduleStatus()
    {
      // Arrange.
      ITestLoggerFactory testLoggerFactory = TestLoggerFactory.Create();

      Mock<IProcessStatusService> mockProcessStatusService = new();
      TestFunction function = new(testLoggerFactory, mockProcessStatusService.Object);

      DateTime expectedNextSchedule = DateTime.UtcNow.AddMinutes(30);
      TimerInfo timerInfo = new()
      {
        ScheduleStatus = new ScheduleStatus
        {
          Next = expectedNextSchedule
        }
      };

      string expectedMessage =
        $"{TestFunctionName} function next timer scheduled at: {expectedNextSchedule:s}.";

      // Act.
      await function.RunBaseAsync(timerInfo);

      // Assert.
      testLoggerFactory.Sink.LogEntries.Single(x => x.LogLevel == LogLevel.Information)
        .Message.Should().Be(expectedMessage);
    }
  }
}
