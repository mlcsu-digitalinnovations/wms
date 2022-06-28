using AutoMapper;
using WmsHub.Business.Models;
using Entities = WmsHub.Business.Entities;

namespace WmsHub.Ui.Models.Profiles
{
	public class EthnicityProfile : Profile
	{
		public EthnicityProfile()
		{
			CreateMap<Entities.Ethnicity, Ethnicity>().ReverseMap();
			CreateMap<IReferral, EthnicityModel>()
				.ForMember(
					dest => dest.SelectedEthnicity,
					opt => opt.MapFrom(src => src.Ethnicity)
				);
		}
	}
}