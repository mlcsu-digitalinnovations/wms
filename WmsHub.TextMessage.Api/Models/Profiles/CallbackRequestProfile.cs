using AutoMapper;

namespace WmsHub.TextMessage.Api.Models.Profiles
{
  public class CallbackRequestProfile : Profile
  {
    public CallbackRequestProfile()
    {
      CreateMap<Business.Models.Notify.CallbackRequest,
       Notify.CallbackPostRequest>()
        .ForMember(dst => dst.Created_at, opt => opt.MapFrom(src =>
          src.CreatedAt))
        .ForMember(dst => dst.Completed_at, opt => opt.MapFrom(src =>
          src.CompletedAt))
        .ForMember(dst => dst.Sent_at, opt => opt.MapFrom(src =>
          src.SentAt))
        .ForMember(dst => dst.Notification_type, opt => opt.MapFrom(src =>
          src.NotificationType))
        .ForMember(dst => dst.Source_number, opt => opt.MapFrom(src =>
          src.SourceNumber))
        .ForMember(dst => dst.Destination_number, opt => opt.MapFrom(src =>
          src.DestinationNumber))
        .ForMember(dst => dst.Date_received, opt => opt.MapFrom(src =>
          src.DateReceived))
       .ReverseMap();

      CreateMap<Business.Models.Notify.CallbackResponse,
        Notify.CallbackPostResponse>();
    }
  }
}