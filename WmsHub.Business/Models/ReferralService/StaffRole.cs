namespace WmsHub.Business.Models.ReferralService
{
  public class StaffRole : BaseModel, IStaffRole
  {
    public string DisplayName { get; set; }
    public int DisplayOrder { get; set; }
  }
}
