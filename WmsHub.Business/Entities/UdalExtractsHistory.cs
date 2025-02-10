using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WmsHub.Business.Entities;

[Table("UdalExtractsHistory")]
public class UdalExtractsHistory
{
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }

  public DateTime? EndDateTime { get; set; }

  [Required]
  public DateTime ExtractDate { get; set; }

  [Required]
  public DateTime ModifiedFrom { get; set; }

  [Required]
  public DateTime ModifiedTo { get; set; }

  public int? NumberOfCoachingInserts { get; set; }

  public int? NumberOfCoachingUpdates { get; set; }
  
  public int? NumberOfModifiedUpdates { get; set; }

  public int? NumberOfProviderEngagementInserts { get; set; }

  public int? NumberOfProviderEngagementUpdates { get; set; }

  public int? NumberOfReferralInserts { get; set; }

  public int? NumberOfReferralUpdates { get; set; }

  public int? NumberOfWeightMeasurementInserts { get; set; }

  public int? NumberOfWeightMeasurementUpdates { get; set; }

  [Required]
  public DateTime StartDateTime { get; set; }

  public IEnumerable<UdalExtract> UdalExtracts { get; set; }
}
