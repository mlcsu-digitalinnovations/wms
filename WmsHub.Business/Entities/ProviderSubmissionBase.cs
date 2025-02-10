using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WmsHub.Business.Entities
{
  public class ProviderSubmissionBase : BaseEntity
  {
    public Guid ProviderId { get; set; }
    public Guid ReferralId { get; set; }
    public int Coaching { get; set; }
    public DateTimeOffset Date { get; set; }
    public int Measure { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Weight { get; set; }
  }
}
