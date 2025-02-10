using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class CallProfile : Profile
  {
    public CallProfile()
    {
      CreateMap<Call, Entities.Call>().ReverseMap();
    }
  }
}