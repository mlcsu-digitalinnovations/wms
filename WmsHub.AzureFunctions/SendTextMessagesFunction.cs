using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using WmsHub.AzureFunctions.Options;
using WmsHub.AzureFunctions.Services;

namespace WmsHub.AzureFunctions;

public class SendTextMessagesFunction(
  ILoggerFactory loggerFactory,
  IProcessStatusService processStatusService,
  IOptions<SendTextMessagesOptions> sendTextMessagesOptions,
  ISendTextMessagesService sendTextMessagesService)
  : AFunctionBase<SendTextMessagesFunction>(loggerFactory, processStatusService)
{
  private readonly ISendTextMessagesService _sendTextMessagesService = sendTextMessagesService;

  protected override string FunctionName => sendTextMessagesOptions.Value.FunctionName;

  [Function("SendTextMessages")]
  public async Task Run([TimerTrigger("%SendTextMessagesTimerTrigger%")] TimerInfo timerInfo)
    => await RunBaseAsync(timerInfo);

  protected override async Task<string> RunProcessAsync()
    => await _sendTextMessagesService.ProcessAsync();
}
