using System;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Common.Models;

public class CriCreateRequest
{
  [Required]
  public string Ubrn { get; set; }
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
