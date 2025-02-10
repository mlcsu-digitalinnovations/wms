using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WmsHub.Business.Models.ReferralService
{
  public interface IGeneralReferralCancel
  {
    string Ethnicity { get; set; }
    bool? HasActiveEatingDisorder { get; set; }
    bool? HasHadBariatricSurgery { get; set; }
    decimal HeightCm { get; set; }
    Guid Id { get; set; }
    bool? IsPregnant { get; set; }
    string ServiceUserEthnicity { get; set; }
    string ServiceUserEthnicityGroup { get; set; }
    decimal WeightKg { get; set; }

    IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
  }
}