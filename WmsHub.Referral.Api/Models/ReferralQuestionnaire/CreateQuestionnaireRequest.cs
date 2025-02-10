using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Referral.Api.Models.ReferralQuestionnaire;

/// <summary>
/// Request Dates.
/// </summary>
public class CreateQuestionnaireRequest
{
  /// <summary>
  /// To Date.
  /// </summary>
  /// <example>"2023-02-24T13:16:28.383Z"</example>
  public DateTimeOffset? ToDate { get; set; }

  /// <summary>
  /// From date.
  /// </summary>
  /// <example>"2022-04-01T13:16:28.383Z"</example>
  public DateTimeOffset? FromDate { get; set; }

  /// <summary>
  /// Max number of questionnaires to create. Maximum of 250.
  /// </summary>
  /// <example>150</example>
  [Range(1, 250)]
  public int MaxNumberToCreate { get; set; }
}
