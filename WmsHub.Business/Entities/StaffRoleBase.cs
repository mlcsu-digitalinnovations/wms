namespace WmsHub.Business.Entities
{
  public abstract class StaffRoleBase : BaseEntity
  {
    public string DisplayName { get; set; }
    public int DisplayOrder { get; set; }
  }
}