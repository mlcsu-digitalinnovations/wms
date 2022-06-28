using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services
{
  public interface IDeprivationService : IServiceBase
  {
    Task EtlImdFile();
    Task<Deprivation> GetByLsoa(string lsoa);
    Task RefreshDeprivations(IEnumerable<Deprivation> deprivations);
  }
}