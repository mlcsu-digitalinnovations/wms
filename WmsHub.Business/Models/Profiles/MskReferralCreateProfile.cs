using AutoMapper;
using WmsHub.Business.Models.ReferralService.MskReferral;

namespace WmsHub.Business.Models.Profiles
{
  internal class MskReferralCreateProfile : Profile
  {
    public MskReferralCreateProfile()
    {
      CreateMap<MskReferralCreate, Entities.Referral>()
        .ForMember(dest => dest.ReferringOrganisationOdsCode,
          o => o.MapFrom(src => src.ReferringMskHubOdsCode))
        .ForMember(dest => dest.ReferringClinicianEmail,
          o => o.MapFrom(src => src.ReferringMskClinicianEmailAddress));
    }
  }
}