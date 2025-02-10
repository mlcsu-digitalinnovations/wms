using AutoMapper;
using WmsHub.Business.Models;
using WmsHub.Business.Models.ReferralService.MskReferral;

namespace WmsHub.Business.Entities.Profiles
{
  public class ReferralProfile : Profile
  {
    public ReferralProfile()
    {
      CreateMap<Referral, Referral>();
      CreateMap<ReferralClinicalInfo, ReferralCri>();
      CreateMap<Referral, Models.Referral>().ReverseMap();
      CreateMap<IMskReferralCreate, Referral>();
    }
  }
}
