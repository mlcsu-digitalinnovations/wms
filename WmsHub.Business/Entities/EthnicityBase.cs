namespace WmsHub.Business.Entities
{
  public abstract class EthnicityBase : BaseEntity
  {
    public string DisplayName { get; set; }
    public string GroupName { get; set; }
    public string OldName { get; set; }
    public string TriageName { get; set; }

    public int GroupOrder { get; set; }
    public int DisplayOrder { get; set; }
    public decimal MinimumBmi { get; set; }
  }
}
