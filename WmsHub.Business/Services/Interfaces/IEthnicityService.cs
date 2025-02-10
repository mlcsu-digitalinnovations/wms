using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services;

public interface IEthnicityService : IServiceBase
{
  Task<IEnumerable<Ethnicity>> GetAsync();

  Task<Ethnicity> GetByMultiple(string ethnicity);

  Task<IEnumerable<string>> GetEthnicityGroupNamesAsync();

  Task<IEnumerable<Ethnicity>> GetEthnicityGroupMembersAsync(string groupName);

  Task<bool> IsBmiValidByTriageNameAsync(string triageName, decimal bmi);
}