using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using WmsHub.Business.Enums;
using WmsHub.Business.Enums.EnumHelper;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Models.PatientTriage
{
  public class PatientTriageUpdateRequest: IValidatableObject
  {
    [Required]
    public string TriageArea { get; set; }
    [Required]
    public string Key { get; set; }
    [Required]
    public int Value { get; set; }

    public IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      if (!Enum.TryParse(TriageArea, out TriageSection _))
        yield return
          new InvalidValidationResult(nameof(TriageArea), TriageArea);

      
    }
  }

  public class PatientTriageUpdateResponse : PatientTriageUpdateRequest
  {
    public virtual StatusType Status { get; set; } = StatusType.Valid;

    public virtual List<string> Errors { get; set; } = new List<string>();

    public virtual string GetErrorMessage()
    {
      string msg = string.Join(" ", Errors);
      return msg;
    }
  }
}
