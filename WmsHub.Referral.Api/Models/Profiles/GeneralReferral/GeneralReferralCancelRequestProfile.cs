using AutoMapper;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Referral.Api.Models.GeneralReferral;

namespace WmsHub.Referral.Api.Models.Profiles.GeneralReferral;

public class GeneralReferralCancelRequestProfile : Profile
{
  public GeneralReferralCancelRequestProfile()
  {
    CreateMap<GeneralReferralCancelRequest, GeneralReferralCancel>();
  }
}