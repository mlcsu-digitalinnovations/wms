using AutoMapper;

namespace WmsHub.Business.Models.Profiles;

public class UdalExtractProfile : Profile
{
  public UdalExtractProfile()
  {
    CreateMap<Entities.UdalExtract, UdalExtract>();
  }
}