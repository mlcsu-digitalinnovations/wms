using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Models.Notify;

public class QuestionnaireNotificationOptions: 
  NotificationOptions,
  IValidatableObject
{
  public string NotificationQuestionnaireLinkUrl =>
    NotificationQuestionnaireLink.EnsureEndsWithForwardSlash();

  public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
  {
    if (string.IsNullOrWhiteSpace(NotificationQuestionnaireLink))
    {
      yield return new ValidationResult(
        $"{nameof(NotificationQuestionnaireLink)} is required."
        );
    }

    if (string.IsNullOrWhiteSpace(NotificationSenderId))
    {
      yield return new ValidationResult(
        "The NotificationOptions.NotificationSenderId property has not " +
        "been set."
        );
    }
  }
}