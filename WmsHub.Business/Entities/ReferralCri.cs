using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Entities.Interfaces;

#nullable enable

namespace WmsHub.Business.Entities
{
  public class ReferralCri : ReferralCriBase, IReferralCri
  {
    public byte[]? ClinicalInfoPdfBase64 { get; set; }
  }
}
