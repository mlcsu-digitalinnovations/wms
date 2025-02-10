using System.Net.Http;
using System.Threading.Tasks;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services;

public interface INotificationService
{
  Task<HttpResponseMessage> GetEmailHistory(string clientReference);
  Task<HttpResponseMessage> GetMessageVerification(string messageId);
  Task<HttpResponseMessage> SendMessageAsync(MessageQueue queueItem);
  Task<SmsPostResponse> SendNotificationAsync(SmsPostRequest request);
}
