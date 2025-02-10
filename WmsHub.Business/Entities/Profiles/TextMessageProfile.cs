using AutoMapper;

namespace WmsHub.Business.Entities.Profiles
{
  public class TextMessageProfile : Profile
  {
    public TextMessageProfile()
    {
      CreateMap<TextMessage, TextMessage>();
    }
  }
}