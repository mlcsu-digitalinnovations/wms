using AutoMapper;
using WmsHub.Business.Models;
using Entities = WmsHub.Business.Entities;

namespace WmsHub.Ui.Models.Profiles
{
  public class UserStoreProfile : Profile
  {
    public UserStoreProfile()
    {
      CreateMap<UserStore, Entities.UserStore>().ReverseMap();
    }
  }
}