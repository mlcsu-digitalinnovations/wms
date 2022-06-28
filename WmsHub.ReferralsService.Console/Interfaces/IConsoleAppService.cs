using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace WmsHub.ReferralsService.Console.Interfaces
{
  public interface IConsoleAppService
  {
    void ConfigureService(IConfiguration configuration);
    Task<int> PerformProcess(string[] options);
  }
}
