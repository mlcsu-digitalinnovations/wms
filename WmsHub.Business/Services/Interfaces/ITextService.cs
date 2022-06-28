using Notify.Models.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Models;
using WmsHub.Business.Models.Notify;

namespace WmsHub.Business.Services
{
  public interface ITextService : IServiceBase
  {
    Task<CallbackResponse> CallBackAsync(ICallbackRequest request);
    void Dispose();
    Task<IEnumerable<ISmsMessage>> GetMessagesToSendAsync(int? limit = null);
    Task<SmsNotificationResponse> SendSmsMessageAsync(ISmsMessage smsMessage);
    Task UpdateMessageRequestAsync(ISmsMessage request, string outcome);
    Task<bool> AddNewMessageAsync(TextMessageRequest message);
    Task<CallbackResponse>
      ReferralMobileNumberInvalidAsync(ICallbackRequest request);

    Task<int> PrepareMessagesToSend(Guid referralId);
    Task<ISmsMessage> GetMessageByReferralIdToSendAsync(Guid referralId);
    }
}