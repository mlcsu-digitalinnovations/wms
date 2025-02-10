using AutoMapper;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Referral.Api.Models.GeneralReferral;

namespace WmsHub.Referral.Api.Models.Profiles.GeneralReferral
{
  public class PutRequestProfile : Profile
  {
    public PutRequestProfile()
    {
      CreateMap<PutRequest, GeneralReferralCreate>();
      CreateMap<PutRequest, GeneralReferralUpdate>();
    }
  }

}