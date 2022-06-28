using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services
{
  public interface IEthnicityService : IServiceBase
  {
    Task<IEnumerable<Ethnicity>> Get();
		Task<IList<string>> GetEthnicityGroupNamesAsync();
		Task<IEnumerable<Models.Ethnicity>> GetEthnicityGroupMembersAsync(
			string groupName
		);
  }
}