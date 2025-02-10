using AutoMapper;
using WmsHub.Business.Models.ReferralService;
using WmsHub.Common.Api.Models;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class GetActiveUbrnResponseProfile : Profile
  {
    public GetActiveUbrnResponseProfile()
    {
      CreateMap<ActiveReferralAndExceptionUbrn, GetActiveUbrnResponse>();
    }
  }
}