using System.Threading.Tasks;

namespace WmsHub.Common.SignalR
{
  public interface ISignalRMessenger
  {
    Task SendChatBotTransfer();
    Task SendChatBotTransferVulnerable();
    Task SendNotification(string message);
  }
}