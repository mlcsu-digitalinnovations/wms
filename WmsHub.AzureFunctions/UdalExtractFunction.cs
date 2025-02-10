using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using WmsHub.AzureFunctions.Services;

namespace WmsHub.AzureFunctions;

public class UdalExtractFunction(
  ILoggerFactory loggerFactory,
  IProcessStatusService processStatusService,
  IUdalExtractService udalExtractService)
  : AFunctionBase<UdalExtractFunction>(loggerFactory, processStatusService)
{
  private readonly IUdalExtractService _udalExtractService = udalExtractService;

  protected override string FunctionName => "WmsHub.AzureFunctions.UdalExtract";

  [Function("UdalExtract")]
  public async Task Run([TimerTrigger("%UdalExtractTimerTrigger%")] TimerInfo timerInfo) 
    => await RunBaseAsync(timerInfo);

  protected override async Task<string> RunProcessAsync() 
    => await _udalExtractService.ProcessAsync();
}
