using AutoMapper;
using WmsHub.Business.Models.ReferralService;

namespace WmsHub.Business.Models.Profiles
{
  class PharmacyReferralCreateProfile : Profile
  {
    public PharmacyReferralCreateProfile()
    {
      CreateMap<PharmacyReferralCreate, Entities.Referral>()
        .ForMember(dest => dest.ReferringOrganisationEmail,
          o => o.MapFrom(src => src.ReferringPharmacyEmail))
        .ForMember(dest => dest.ReferringOrganisationOdsCode,
          o => o.MapFrom(src => src.ReferringPharmacyOdsCode));
    }
  }
}