using System;

namespace WmsHub.Business.Models.ProviderService
{
  public class ProviderRejectionReasonUpdate
  {
    public Guid? Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool? IsActive { get; set; }
  }
}