namespace WmsHub.Business.Models
{
  public class Ethnicity : BaseModel, IEthnicity
  {
    public string DisplayName { get; set; }
    public string GroupName { get; set; }
    public string OldName { get; set; }
    public string TriageName { get; set; }
    public decimal? MinimumBmi { get; set; }
    public int GroupOrder { get; set; }
    public int DisplayOrder { get; set; }
  }
}