using AutoMapper;
using WmsHub.Business.Models;
using WmsHub.Common.Api.Models;

namespace WmsHub.Referral.Api.Models.Profiles
{
  public class ReferralNhsNumberMismatchPostProfile : Profile
  {
    public ReferralNhsNumberMismatchPostProfile()
    {
      CreateMap<ReferralNhsNumberMismatchPost, ReferralExceptionCreate>();
      CreateMap<ReferralNhsNumberMismatchPost, ReferralExceptionUpdate>();
    }
  }
}