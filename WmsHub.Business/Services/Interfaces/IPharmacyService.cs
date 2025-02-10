using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services
{
  public interface IPharmacyService : IServiceBase
  {
    Task<IPharmacy> CreateAsync(IPharmacy createModel);
    Task<Pharmacy> GetByObsCodeAsync(string odsCode);
    Task<IPharmacy> UpdateAsync(IPharmacy updateModel);
  }
}