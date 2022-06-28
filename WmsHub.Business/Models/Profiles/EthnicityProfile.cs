using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class EthnicityProfile : Profile
  {
    public EthnicityProfile()
    {
      CreateMap<Ethnicity, Entities.Ethnicity>().ReverseMap();
    }
  }
}