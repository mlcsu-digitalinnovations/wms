using System;

namespace WmsHub.Business.Entities
{
  public class PharmacistBase : BaseEntity
  {
    public string ReferringPharmacyEmail { get; set; }
    public string KeyCode { get; set; }
    public DateTimeOffset? Expires { get; set; }
    public string IPAddress { get; set; }
    public int TryCount { get; set; }
  }
}