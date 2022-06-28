using AutoMapper;
using WmsHub.Business.Models.ProviderRejection;
using WmsHub.Business.Models.ProviderService;

namespace WmsHub.Business.Models.Profiles
{
  public class ProviderRejectionReasonProfile : Profile
  {
    public ProviderRejectionReasonProfile()
    {
      CreateMap<Entities.ProviderRejectionReason,
        ProviderRejectionReason>().ReverseMap();

      CreateMap<ProviderRejectionReasonSubmission,
        Entities.ProviderRejectionReason>();

      CreateMap<Entities.ProviderRejectionReason,
        ProviderRejectionReasonResponse>();
    }
  }
}