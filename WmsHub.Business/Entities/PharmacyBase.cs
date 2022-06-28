namespace WmsHub.Business.Entities
{
  public class PharmacyBase : BaseEntity
  {
    public string Email { get; set; }
    public string OdsCode { get; set; }
    public string TemplateVersion { get; set; }
  }
}