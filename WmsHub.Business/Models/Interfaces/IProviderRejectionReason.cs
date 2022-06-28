using System;

namespace WmsHub.Business.Models
{
  public interface IProviderRejectionReason
  {
    string Title { get; set; }
    string Description { get; set; }
    Guid Id { get; set; }
    bool IsActive { get; set; }
    DateTimeOffset ModifiedAt { get; set; }
    Guid ModifiedByUserId { get; set; }
  }
}