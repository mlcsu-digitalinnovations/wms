using System.Threading.Tasks;
using WmsHub.Common.Models;

namespace WmsHub.ReferralsService.Interfaces
{
  public interface ISmartCardAuthentictor
  {
    ErsSession ActiveSession { get; set; }

    void ConnectToSmartCard();
    Task<bool> CreateSession();
    Task<bool> TerminateSession();

  }
}
