using AutoMapper;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class ReferralDischargeProfile : Profile
  {
    public ReferralDischargeProfile()
    {
      CreateMap<Business.Models.ReferralDischarge,
        Common.Api.Models.ReferralDischarge>().ReverseMap();
    }
  }
}