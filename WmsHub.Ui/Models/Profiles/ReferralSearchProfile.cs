using AutoMapper;

namespace WmsHub.Ui.Models.Profiles
{
	public class ReferralSearchProfile : Profile
	{
		public ReferralSearchProfile()
		{
			CreateMap<Business.Models.ReferralSearch, ReferralSearchModel>()
				.ReverseMap();
		}
	}
}