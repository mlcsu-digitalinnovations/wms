namespace WmsHub.Business.Models
{
  public interface IEthnicity : IBaseModel
  {
    string DisplayName { get; set; }
    string GroupName { get; set; }
    string OldName { get; set; }
    string TriageName { get; set; }
    public decimal? MinimumBmi { get; set; }
  }
}