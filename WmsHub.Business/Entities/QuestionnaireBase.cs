using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Entities;

public class QuestionnaireBase : BaseEntity
{
  public DateTimeOffset EndDate { get; set; }
  public Guid NotificationTemplateId { get; set; }
  public DateTimeOffset StartDate { get; set; }
  public QuestionnaireType Type { get; set; }
}
