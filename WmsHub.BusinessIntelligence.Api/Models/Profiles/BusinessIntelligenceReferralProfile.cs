using AutoMapper;

namespace WmsHub.BusinessIntelligence.Api.Models.Profiles
{
    public class BusinessIntelligenceReferralProfile : Profile
  {
    public BusinessIntelligenceReferralProfile()
    {
      CreateMap<Business.Models.BusinessIntelligence.BusinessIntelligenceData, 
        BusinessIntelligenceData>().ReverseMap();
    }
  }
}