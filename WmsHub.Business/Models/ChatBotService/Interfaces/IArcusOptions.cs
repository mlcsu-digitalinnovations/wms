using System.Collections.Generic;
using WmsHub.Common.Models;

namespace WmsHub.Business.Models.ChatBotService
{
  public interface IArcusOptions: INumberWhiteListOptions
  {
    string ApiKey { get; set; }
    string ContactFlowName { get; set; }
    string Endpoint { get; set; }
  }
}