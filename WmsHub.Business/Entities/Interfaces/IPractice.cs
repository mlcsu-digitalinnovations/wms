namespace WmsHub.Business.Entities
{
  public interface IPractice : IBaseEntity
  {
    string Email { get; set; }
    string Name { get; set; }
    string OdsCode { get; set; }
    string SystemName { get; set; }
  }
}