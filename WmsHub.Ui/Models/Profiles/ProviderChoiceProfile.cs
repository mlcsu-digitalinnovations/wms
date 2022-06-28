using AutoMapper;
using WmsHub.Business.Models;

namespace WmsHub.Ui.Models.Profiles
{
	public class ProviderChoiceProfile : Profile
	{
		public ProviderChoiceProfile()
		{
			CreateMap<IReferral, ProviderChoiceModel>();
		}
	}
}