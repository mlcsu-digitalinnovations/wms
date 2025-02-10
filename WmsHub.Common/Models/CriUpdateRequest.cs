using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Models;

public class CriUpdateRequest
{
  /// <summary>
  /// Byte Array of Clinical Information PDF
  /// </summary>
  [Required]
  public string CriDocument { get; set; }
  /// <summary>
  /// Date of last update of Clinical Information provided
  /// </summary>
  [Required]
  public DateTimeOffset? ClinicalInfoLastUpdated { get; set; }
}
