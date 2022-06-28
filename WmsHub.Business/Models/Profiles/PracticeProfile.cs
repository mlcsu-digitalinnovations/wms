using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class PracticeProfile : Profile
  {
    public PracticeProfile()
    {
      CreateMap<Practice, Entities.Practice>().ReverseMap();
    }
  }
}