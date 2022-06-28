using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WmsHub.Referral.Api.Models
{
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
}
