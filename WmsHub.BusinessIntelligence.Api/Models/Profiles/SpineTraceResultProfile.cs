using AutoMapper;

namespace WmsHub.BusinessIntelligence.Api.Models.Profiles
{
  public class SpineTraceResultProfile : Profile
  {
    public SpineTraceResultProfile()
    {
      CreateMap<Business.Models.SpineTraceResult, SpineTraceResult>()
        .ReverseMap();
    }
  }
}