using System;

namespace WmsHub.Business.Entities.Interfaces
{
  public interface IPharmacist:IBaseEntity
  {
    string ReferringPharmacyEmail { get; set; }
    string KeyCode { get; set; }
    DateTimeOffset? Expires { get; set; }
    string IPAddress { get; set; }
    int TryCount { get; set; }
  }
}