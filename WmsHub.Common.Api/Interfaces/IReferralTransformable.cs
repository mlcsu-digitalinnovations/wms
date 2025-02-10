using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Common.Api.Interfaces
{
  public interface IReferralTransformable
  {
    string Address1 { get; set; }
    string Address2 { get; set; }
    string Address3 { get; set; }
    string Postcode { get; set; }
    bool? IsVulnerable { get; set; }
    bool? HasAPhysicalDisability { get; set; }
    bool? HasALearningDisability { get; set; }
    bool? HasRegisteredSeriousMentalIllness { get; set; }
    bool? HasHypertension { get; set; }
    bool? HasDiabetesType1 { get; set; }
    bool? HasDiabetesType2 { get; set; }
    
  }
}
