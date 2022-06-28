using AutoMapper;
using WmsHub.Business.Helpers;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class CRiUpdateRequestProfile : Profile
  {
    public CRiUpdateRequestProfile()
    {
      CreateMap<CriUpdateRequest, Business.Models.ReferralClinicalInfo>()
        .ForMember(dest => dest.ClinicalInfoPdfBase64,
          opt => opt.MapFrom(src => src.CriDocument.CompressFromBase64()));

    }
  }
}