using System;
using System.Linq.Expressions;
namespace WmsHub.Business.Models
{
  public class RequestResponseLog : IRequestResponseLog
  {
    public RequestResponseLog() { }
    public RequestResponseLog(Entities.RequestResponseLog entity)
    {
      if (entity == null) return;

      Action = entity.Action;
      Controller = entity.Controller;
      Id = entity.Id;
      Request = entity.Request;
      RequestAt = entity.RequestAt;
      Response = entity.Response;
      ResponseAt = entity.ResponseAt;
      UserId = entity.UserId;
    }

    public string Action { get; set; }
    public string Controller { get; set; }
    public int Id { get; set; }
    public string Request { get; set; }
    public DateTimeOffset RequestAt { get; set; }
    public string Response { get; set; }
    public DateTimeOffset ResponseAt { get; set; }
    public Guid? UserId { get; set; }

    public void MapToEntity(Entities.RequestResponseLog entity)
    {
      if (entity == null) return;

      entity.Action = Action;
      entity.Controller = Controller;
      entity.Id = Id;
      entity.Request = Request;
      entity.RequestAt = RequestAt;
      entity.Response = Response;
      entity.ResponseAt = ResponseAt;
      entity.UserId = UserId;
    }

    public static Expression<
      Func<Entities.RequestResponseLog, RequestResponseLog>> ProjectFromEntity
    {
      get
      {
        return entity => new RequestResponseLog(entity);
      }
    }
  }
}