using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services
{
  public interface IRequestResponseLogService
  {
    Task CreateAsync(IRequestResponseLog model);
  }
}