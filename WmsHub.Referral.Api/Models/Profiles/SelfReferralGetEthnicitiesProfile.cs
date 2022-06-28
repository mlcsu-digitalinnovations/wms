using AutoMapper;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class SelfReferralGetEthnicitiesProfile : Profile
  {
    public SelfReferralGetEthnicitiesProfile()
    {
      CreateMap<Business.Models.Ethnicity, Ethnicity>();
    }
  }
}