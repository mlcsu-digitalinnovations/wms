using System;

namespace WmsHub.Business.Models
{
  public class BaseModel : IBaseModel
  {
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public Guid ModifiedByUserId { get; set; }
  }
}