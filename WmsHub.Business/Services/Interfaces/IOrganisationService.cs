using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services;
public interface IOrganisationService : IServiceBase
{
  Task<Organisation> AddAsync(Organisation organisation);
  Task DeleteAsync(string odsCode);
  Task<IEnumerable<Organisation>> GetAsync();
  Task ResetOrganisationQuotas();
  Task<Organisation> UpdateAsync(Organisation organisation);
}
