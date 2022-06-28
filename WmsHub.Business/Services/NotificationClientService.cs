using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notify.Client;
using Notify.Exceptions;
using Notify.Interfaces;
using WmsHub.Business.Models;
using WmsHub.Business.Models.AuthService;

namespace WmsHub.Business.Services
{
  public class NotificationClientService : INotificationClientService
  {
    public virtual async Task<bool> SendKeyUsingSmsAsync(Provider model,
      string smsKey, string smsTemplateId, string senderId)
    {
      IAsyncNotificationClient textClient =
        new NotificationClient(smsKey);

      Object response =
        await textClient.SendSmsAsync(
          mobileNumber: model.ProviderAuth.MobileNumber,
          templateId: smsTemplateId,
          personalisation: new Dictionary<string, dynamic>
          {
            {"code", model.ProviderAuth.SmsKey}
          },
          clientReference: model.Id.ToString(),
          smsSenderId: senderId
        ) as Object;

      if (response.GetType() == typeof(NotifyClientException))
      {
        throw (NotifyClientException) response;
      }

      return true;
    }

  }
}
