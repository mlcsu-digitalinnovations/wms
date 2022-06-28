using System.Threading.Tasks;
using WmsHub.Business.Entities;

namespace WmsHub.Business.Services
{
  public interface IUserActionLogService
  {
    Task CreateAsync(IUserActionLog entity);
  }
}