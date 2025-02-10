using WmsHub.Business.Models.Interfaces;

namespace WmsHub.Business.Models.ReferralService
{
  public interface IStaffRole : IBaseModel
  {
    string DisplayName { get; set; }
    int DisplayOrder { get; set; }
  }
}