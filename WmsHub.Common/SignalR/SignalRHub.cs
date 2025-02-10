using Microsoft.AspNetCore.SignalR;

namespace WmsHub.Common.SignalR
{
	public class SignalRHub : Hub
	{
		public async void UpdateReferralActionStatus(
			string activeUser,
			string referralId)
		{
			await Clients.All
				.SendAsync("UpdateReferralActionStatus", activeUser, referralId);
		}

		public async void ClearReferralActionStatus(string activeUser)
		{
			await Clients.All.SendAsync("ClearReferralActionStatus", activeUser);
		}

		public async void NewReferral()
		{
			await Clients.All.SendAsync("NewReferral");
		}

		public async void NewVulnerableReferral()
		{
			await Clients.All.SendAsync("NewVulnerableReferral");
		}
	}
}