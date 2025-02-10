namespace WmsHub.Business.Models
{
  public interface IStatus
  {
    string Status { get; set; }
    string StatusReason { get; set; }
  }
}