using AutoMapper;
using WmsHub.Business.Models;
using WmsHub.Referral.Api.Models.GeneralReferral;

namespace WmsHub.Referral.Api.Models.Profiles.GeneralReferral
{
  public class GetNhsNumberOkResponseProfile : Profile
  {
    public GetNhsNumberOkResponseProfile()
    {
      CreateMap<IReferral, GetNhsNumberOkResponse>();
    }
  }
}