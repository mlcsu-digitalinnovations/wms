using AutoMapper;

namespace WmsHub.Business.Models.Profiles;

public class SpineTraceReponseProfile : Profile
{
  public SpineTraceReponseProfile()
  {
    CreateMap<SpineTraceResult, SpineTraceResponse>().ReverseMap();
  }
}