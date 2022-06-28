using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Entities
{
  public class UserActionLog : IUserActionLog
  {
    public string Action { get; set; }
    public string Controller { get; set; }
    [Key]
    public int Id { get; set; }
    public string IpAddress { get; set; }
    public string Method { get; set; }
    public string Request { get; set; }
    public DateTimeOffset RequestAt { get; set; }
    public Guid UserId { get; set; }
  }
}