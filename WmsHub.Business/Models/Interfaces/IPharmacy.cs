namespace WmsHub.Business.Models
{
  public interface IPharmacy : IBaseModel
  {
    string Email { get; set; }
    string OdsCode { get; set; }
    string TemplateVersion { get; set; }
  }
}