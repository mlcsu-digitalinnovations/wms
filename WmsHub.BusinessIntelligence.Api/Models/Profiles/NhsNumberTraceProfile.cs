using AutoMapper;

namespace WmsHub.BusinessIntelligence.Api.Models.Profiles
{
  public class NhsNumberTraceProfile : Profile
  {
    public NhsNumberTraceProfile()
    {
      CreateMap<Business.Models.NhsNumberTrace, NhsNumberTrace>().ReverseMap();
    }
  }
}