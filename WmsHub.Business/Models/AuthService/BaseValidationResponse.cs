using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WmsHub.Business.Enums;

namespace WmsHub.Business.Models.AuthService
{
  public class BaseValidationResponse
  {
    public virtual ValidationType ValidationStatus { get; set; }

    public List<string> Errors { get; private set; } = new List<string>();

    public string GetErrorMessage()
    {
      string msg = string.Join(" ", Errors);
      return msg;
    }

    public void SetStatus(ValidationType validationStatus, string errorMessage)
    {
      ValidationStatus = validationStatus;
      Errors.Add(errorMessage);
    }
  }
}
