using System.Security.Claims;
using System.Threading.Tasks;

namespace WmsHub.Business.Services
{
  public interface IServiceBase
  {
    public Task<int> ActivateAsync(int id);
    public Task<int> DeactivateAsync(int id);
    public ClaimsPrincipal User { get; set; }    
  }
}