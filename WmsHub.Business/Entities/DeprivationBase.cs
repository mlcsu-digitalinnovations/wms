namespace WmsHub.Business.Entities
{
  public abstract class DeprivationBase : BaseEntity, IBaseEntity
  {
    public int ImdDecile { get; set; }
    public string Lsoa { get; set; }
  }
}
