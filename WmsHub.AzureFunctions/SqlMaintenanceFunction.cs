using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using WmsHub.AzureFunctions.Services;

namespace WmsHub.AzureFunctions;

public class SqlMaintenanceFunction(
  ILoggerFactory loggerFactory,
  IProcessStatusService processStatusService,
  ISqlMaintenanceService sqlMaintenanceService)
  : AFunctionBase<SqlMaintenanceFunction>(loggerFactory, processStatusService)
{
  protected override string FunctionName => "WmsHub.AzureFunctions.SqlMaintenance";

  private readonly ISqlMaintenanceService _sqlMaintenanceService = sqlMaintenanceService;

  [Function("SqlMaintenance")]
  public async Task Run([TimerTrigger("%SqlMaintenanceTimerTrigger%")] TimerInfo timerInfo) 
    => await RunBaseAsync(timerInfo);

  protected override async Task<string> RunProcessAsync() 
    => await _sqlMaintenanceService.ProcessAsync();
}
