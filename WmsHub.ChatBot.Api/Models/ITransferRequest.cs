using System;

namespace WmsHub.ChatBot.Api.Models
{
  public interface ITransferRequest
  {
    string TransferOutcome { get; set; }
    DateTimeOffset? Timestamp { get; set; }
    string Number { get; set; }
  }
}
