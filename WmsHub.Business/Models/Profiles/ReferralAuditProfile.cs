using AutoMapper;

namespace WmsHub.Business.Models.Profiles
{
  public class ReferralAuditProfile : Profile
  {
    public ReferralAuditProfile()
    {
      CreateMap<Entities.ReferralAudit[], ReferralAudit[]>();
      CreateMap<Entities.ReferralAudit, ReferralAudit>();
      // provider name not currently used
      //.ForMember(d => d.ProviderName, o => o.MapFrom(s => s.Provider.Name));
    }
  }
}