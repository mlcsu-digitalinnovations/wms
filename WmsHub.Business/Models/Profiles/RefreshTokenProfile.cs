using AutoMapper;
using WmsHub.Business.Models.AuthService;

namespace WmsHub.Business.Models.Profiles
{
  public class RefreshTokenProfile : Profile
  {
    public RefreshTokenProfile()
    {
      CreateMap<RefreshToken, Entities.RefreshToken>().ReverseMap();
    }
  }
}
