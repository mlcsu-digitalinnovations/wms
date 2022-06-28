using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class TextMessageProfile : Profile
  {
    public TextMessageProfile()
    {
      CreateMap<TextMessage, Entities.TextMessage>().ReverseMap();
    }
  }
}
