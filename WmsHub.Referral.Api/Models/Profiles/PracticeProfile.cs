using AutoMapper;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class PracticeProfile : Profile
  {
    public PracticeProfile()
    {
      CreateMap<Practice, Business.Models.Practice>().ReverseMap();
      //CreateMap<Practice, Business.Models.IPractice>().ReverseMap();
    }
  }
}