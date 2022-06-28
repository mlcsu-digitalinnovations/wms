using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WmsHub.Common.SignalR
{
  public class SignalRMessenger : ISignalRMessenger
  {
    private readonly IHubContext<SignalRHub> _hubContext;

    public SignalRMessenger(IHubContext<SignalRHub> hubContext)
    {
      _hubContext = hubContext;
    }

    public async Task SendNotification(string message)
    {
      await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
    }

    public async Task SendChatBotTransfer()
    {
      await _hubContext.Clients.All.SendAsync("NewReferral");
    }

    public async Task SendChatBotTransferVulnerable()
    {
      await _hubContext.Clients.All.SendAsync("NewVulnerableReferral");
    }
  }
}