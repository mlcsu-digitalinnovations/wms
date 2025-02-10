using AutoMapper;
using WmsHub.Business.Models;

namespace WmsHub.Ui.Models.Profiles;

	public class ServiceUserProfile : Profile
	{
		public ServiceUserProfile()
  {
    CreateMap<ServiceUserModel, IReferral>()
      .ForMember(dest => dest.Email,
        opt => opt.MapFrom(src => src.EmailAddress));
    CreateMap<IReferral, ServiceUserModel>()
      .ForMember(dest => dest.EmailAddress,
        opt => opt.MapFrom(src =>
          src.Email.Equals(Common.Helpers.Constants.DO_NOT_CONTACT_EMAIL)
            ? ""
            : src.Email))
      .ForMember(dest => dest.Source,
        opt => opt.MapFrom(src =>
          src.ReferralSource))
      .ForMember(dest => dest.SelectedProvider,
        opt => opt.MapFrom(src => src.Provider));
  }
	}