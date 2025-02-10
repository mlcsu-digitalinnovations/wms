using System.Net.Http;
using System.Threading.Tasks;
using WmsHub.Business.Models.ChatBotService;

namespace WmsHub.ChatBot.Api.Clients
{
  public interface IArcusClientHelper
  {
    Task<HttpResponseMessage> BatchPost(IArcusCall request);
    void Dispose();
    HttpRequestMessage GetBlankPutRequest(string path);
    HttpRequestMessage GetPutObjectRequestAsJson(string json, string path);
  }
}