using System.Threading.Tasks;

namespace WmsHub.Business.Services
{
  public interface IReferralDischargeService : IServiceBase
  {
    Task<string> PrepareDischarges();
  }
}