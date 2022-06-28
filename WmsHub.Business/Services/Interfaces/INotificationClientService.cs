using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services
{
  public interface INotificationClientService
  {
    Task<bool> SendKeyUsingSmsAsync(Provider model, 
      string smsKey, string smsTemplateId, string senderId);
  }
}