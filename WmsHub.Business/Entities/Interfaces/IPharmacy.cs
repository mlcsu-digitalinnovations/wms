namespace WmsHub.Business.Entities
{
  public interface IPharmacy : IBaseEntity
  {
    string Email { get; set; }
    string OdsCode { get; set; }
    string TemplateVersion { get; set; }
  }
}