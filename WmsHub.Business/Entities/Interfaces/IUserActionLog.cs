using System;

namespace WmsHub.Business.Entities
{
  public interface IUserActionLog
  {
    string Action { get; set; }
    string Controller { get; set; }
    string IpAddress { get; set; }
    string Method { get; set; }
    int Id { get; set; }
    string Request { get; set; }
    DateTimeOffset RequestAt { get; set; }
    Guid UserId { get; set; }
  }
}