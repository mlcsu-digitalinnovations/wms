using AutoMapper;
using WmsHub.Business.Models;
using WmsHub.Referral.Api.Models.ReferralQuestionnaire;

namespace WmsHub.Referral.Api.Models.Profiles.ReferralQuestionnaire;

public class NotificationProxyCallbackRequestProfile : Profile
{
  public NotificationProxyCallbackRequestProfile()
  {
    CreateMap<NotificationProxyCallbackRequest,
      NotificationProxyCallback>();
  }
}
