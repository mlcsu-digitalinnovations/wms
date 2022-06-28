using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Common.Api.Models;
using WmsHub.Common.Helpers;

namespace WmsHub.ReferralsService.Models
{
  /// <summary>
  /// This class is needed in order for the CSV processor to read the UBRN 
  /// into a compatible ReferralPut class, which has no field available.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class ReferralPutCSV : ReferralPut
  {
    public string Ubrn { get; set; }
  }
}
