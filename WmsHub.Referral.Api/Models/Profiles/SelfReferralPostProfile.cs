using AutoMapper;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class SelfReferralPostProfile : Profile
  {
    public SelfReferralPostProfile()
    {
      CreateMap<SelfReferralPostRequest, SelfReferralCreate>();
    }
  }
}