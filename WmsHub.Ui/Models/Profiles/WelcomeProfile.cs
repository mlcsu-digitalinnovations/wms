using AutoMapper;
using WmsHub.Business.Models;

namespace WmsHub.Ui.Models.Profiles
{
	public class WelcomeProfile : Profile
	{
		public WelcomeProfile()
		{
			CreateMap<IReferral, WelcomeModel>()
				.ForMember(
					dest => dest.DisplayName,
					opt => opt.MapFrom(src => src.GivenName)
				);
		}
	}
}