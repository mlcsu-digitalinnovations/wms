using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;

namespace WmsHub.AzureFunctions;

public abstract class AFunctionBase<T>(
  ILoggerFactory loggerFactory,
  IProcessStatusService processStatusService) where T : class
{
  protected abstract string FunctionName { get; }

  private readonly ILogger _logger = loggerFactory.CreateLogger<T>();
  private readonly IProcessStatusService _processStatusService = processStatusService;

  protected abstract Task<string> RunProcessAsync();

  protected async Task RunBaseAsync(TimerInfo timerInfo)
  {
    _logger.LogDebug(
      "{FunctionName} function executed at: {Now:s}.",
      FunctionName,
      DateTime.UtcNow);

    if (timerInfo.ScheduleStatus is not null)
    {
      _logger.LogInformation(
        "{FunctionName} function next timer scheduled at: {Next:s}.",
        FunctionName,
        timerInfo.ScheduleStatus.Next);
    }

    _logger.LogDebug("Sending Started notification to process status.");

    _processStatusService.AppName = FunctionName;
    await _processStatusService.StartedAsync();

    try
    {
      _logger.LogDebug("Running {FunctionName} process.", FunctionName);

      string result = await RunProcessAsync();

      _logger.LogDebug(
        "{FunctionName} process completed with '{Result}'.", 
        FunctionName, 
        result);

      _logger.LogDebug("Sending Success notification to process status.");

      await _processStatusService.SuccessAsync(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "{FunctionName} process failed.", FunctionName);

      _logger.LogDebug("Sending Failure notification to process status.");

      await _processStatusService.FailureAsync(ex.Message);
    }
  }
}
