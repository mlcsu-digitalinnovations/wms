using System;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Entities;

public class ReferralQuestionnaireBase : BaseEntity
{
  public Guid ReferralId { get; set; }
  public Guid QuestionnaireId { get; set; }
  public string NotificationKey { get; set; }
  public DateTimeOffset Created { get; set; }
  public DateTimeOffset? Sending { get; set; }
  public DateTimeOffset? Delivered { get; set; }
  public DateTimeOffset? TemporaryFailure { get; set; }
  public DateTimeOffset? TechnicalFailure { get; set; }
  public DateTimeOffset? PermanentFailure { get; set; }
  public DateTimeOffset? Started { get; set; }
  public DateTimeOffset? Completed { get; set; }
  public int FailureCount { get; set; }
  public string Answers { get; set; }
  public string FamilyName { get; set; }
  public string GivenName { get; set; }
  public string Mobile { get; set; }
  public string Email { get; set; }
  public bool ConsentToShare { get; set; }

  public ReferralQuestionnaireStatus Status { get; set; }

}
