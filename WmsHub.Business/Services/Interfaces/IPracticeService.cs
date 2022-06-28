using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services
{
  public interface IPracticeService : IServiceBase
  {
    Task<IPractice> CreateAsync(IPractice practiceCreate);
    Task<Practice> GetByObsCodeAsync(string odsCode);
    Task<IPractice> UpdateAsync(IPractice practiceUpdate);
  }
}