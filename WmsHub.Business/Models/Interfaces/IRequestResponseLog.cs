using System;

namespace WmsHub.Business.Models
{
  public interface IRequestResponseLog
  {
    string Action { get; set; }
    string Controller { get; set; }
    int Id { get; set; }
    string Request { get; set; }
    DateTimeOffset RequestAt { get; set; }
    string Response { get; set; }
    DateTimeOffset ResponseAt { get; set; }
    Guid? UserId { get; set; }

    void MapToEntity(Entities.RequestResponseLog entity);
  }
}