using System;
using System.Threading.Tasks;
using WmsHub.Business.Enums;
using WmsHub.Business.Models;

namespace WmsHub.Business.Services;

public interface IReferralQuestionnaireService : IServiceBase
{
  Task<CreateReferralQuestionnaireResponse> CreateAsync(
    DateTimeOffset? fromDate,
    int maxNumberToCreate,
    DateTimeOffset toDate);
  Task<SendReferralQuestionnaireResponse> SendAsync();
  Task<StartReferralQuestionnaire> StartAsync(
    string notificationKey);
  Task<CompleteQuestionnaireResponse> CompleteAsync(
    CompleteQuestionnaire request);
  Task<NotificationCallbackStatus> CallbackAsync(
    NotificationProxyCallback request);
}
