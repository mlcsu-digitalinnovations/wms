using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Models.ProviderService
{
  [Obsolete("Please use the ProviderAuthUpdateRequest")]
  public class ProviderUpdateRequest
  {
    [EmailAddress]
    public string Email { get; set; }
    public bool UseEmailForToken { get; set; }
    [RegularExpression(Constants.REGEX_MOBILE_PHONE_UK, 
      ErrorMessage = "The field Mobile is not a valid UK mobile number.")]
    public string MobileNumber { get; set; }
    public bool UseMobileForToken { get; set; }
  }
}
