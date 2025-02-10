using AutoMapper;
using WmsHub.Business.Models.ChatBotService;

namespace WmsHub.ChatBot.Api.Models.Profiles
{
  public class ReferralCallProfile : Profile
  {
    public ReferralCallProfile()
    {
      CreateMap<ReferralCall, UpdateReferralWithCallRequest>();
      CreateMap<ReferralCallStart, GetReferralCallListRequest>();
    }
  }
}