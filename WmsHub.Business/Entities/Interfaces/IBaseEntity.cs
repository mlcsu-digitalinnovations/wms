using System;

namespace WmsHub.Business.Entities
{
  public interface IBaseEntity
  {
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public Guid ModifiedByUserId { get; set; }
  }
}