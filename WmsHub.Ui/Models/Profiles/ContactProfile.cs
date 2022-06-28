using AutoMapper;
using WmsHub.Business.Models;

namespace WmsHub.Ui.Models.Profiles
{
	public class ContactProfile : Profile
	{
		public ContactProfile()
    {
      CreateMap<IReferral, ContactModel>()
        .ForMember(
          dest => dest.CanContact,
          opt => opt.MapFrom(src => src.ConsentForFutureContactForEvaluation)
        )
        .ForMember(
          dest => dest.Email,
          opt => opt.MapFrom(src =>
            src.Email.Equals(Common.Helpers.Constants.DO_NOT_CONTACT_EMAIL) 
              ? "":src.Email));
    }
	}
}