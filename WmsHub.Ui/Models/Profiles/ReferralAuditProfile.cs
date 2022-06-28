using System.Collections.Generic;
using AutoMapper;
using WmsHub.Business.Models;

namespace WmsHub.Ui.Models.Profiles
{
  public class ReferralAuditProfile : Profile
  {
    public ReferralAuditProfile()
    {
      CreateMap<ReferralAudit,ReferralAuditListItemModel>();
      CreateMap<Business.Entities.ReferralAudit, ReferralAudit>();
    }
  }
}