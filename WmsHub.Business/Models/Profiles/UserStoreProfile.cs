using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class UserStoreProfile : Profile
  {
    public UserStoreProfile()
    {
      CreateMap<UserStore, Entities.UserStore>().ReverseMap();
    }
  }
}