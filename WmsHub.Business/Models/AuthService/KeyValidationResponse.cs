using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.AuthService
{
  public class KeyValidationResponse: BaseValidationResponse
  {
    public KeyValidationResponse()
    { }

    public KeyValidationResponse(ValidationType validationStatus,
      string errorMessage = "")
    {
      if (string.IsNullOrWhiteSpace(errorMessage))
        ValidationStatus = validationStatus;
      else
        SetStatus(validationStatus, errorMessage);
    }
   
  }
}
