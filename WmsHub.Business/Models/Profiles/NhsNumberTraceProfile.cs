using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class NhsNumberTraceProfile : Profile
  {
    public NhsNumberTraceProfile()
    {
      CreateMap<Entities.Referral, NhsNumberTrace>();
    }
  }
}