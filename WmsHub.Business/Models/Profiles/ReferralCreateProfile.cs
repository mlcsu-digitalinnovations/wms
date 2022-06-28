using AutoMapper;
using WmsHub.Business.Helpers;

namespace WmsHub.Business.Models.Profiles
{
  public class ReferralCreateProfile : Profile
  {
    public ReferralCreateProfile()
    {
      CreateMap<ReferralCreate, Entities.Referral>()
        .ForMember(dest => dest.Cri, opt => opt.MapFrom((src, dest) =>
        {
          if (src.CriLastUpdated != null &&
              !string.IsNullOrEmpty(src.CriDocument))
            return new Entities.ReferralCri
            {
              ClinicalInfoLastUpdated = src.CriLastUpdated.Value,
              ClinicalInfoPdfBase64 = src.CriDocument.CompressFromBase64()
            };
          return null;
        }));
    }
  }
}