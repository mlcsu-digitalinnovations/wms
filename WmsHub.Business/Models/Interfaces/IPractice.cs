namespace WmsHub.Business.Models
{
  public interface IPractice : IBaseModel
  {
    string Email { get; set; }
    string Name { get; set; }
    string OdsCode { get; set; }
    string SystemName { get; set; }
  }
}