using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Models
{
  [ExcludeFromCodeCoverage]
  public class ReferralClinicalInfo: BaseModel
  {
    public string Ubrn { get; set; }
    public byte[] ClinicalInfoPdfBase64 { get; set; }
    public DateTimeOffset ClinicalInfoLastUpdated { get; set; }
  }
}
