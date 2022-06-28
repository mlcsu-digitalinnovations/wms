using System.Collections.Generic;

namespace WmsHub.Business.Models.ChatBotService
{
  public interface IArcusCall
  {
    IEnumerable<ICallee> Callees { get; set; }
    string ContactFlowName { get; set; }
    string Mode { get; set; }
  }
}