using Microsoft.Extensions.Configuration;
using Mlcsu.Diu.Mustard.Apis.ProcessStatus;
using Mlcsu.Diu.Mustard.Email;
using System.Net.Http;
using System.Threading.Tasks;

namespace WmsHub.ReferralsService.Console.Interfaces
{
  public interface IConsoleAppService
  {
    void ConfigureService(
      IConfiguration configuration, 
      IHttpClientFactory httpClientFactory,
      IProcessStatusService processStatusService,
      ISendEmailService sendEmailService);
    Task<int> PerformProcess(string[] options);
  }
}
