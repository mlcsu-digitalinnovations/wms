using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WmsHub.Common.Validation
{
  public class ValidateModelResult
  {
    public bool IsValid { get; set; }
    public List<ValidationResult> Results { get; set; }

    public ValidateModelResult()
    {
      IsValid = false;
      Results = new List<ValidationResult>();
    }

    public string GetErrorMessage()
    {
      string results = null;
      if (!IsValid && Results.Any())
      {
        results = string.Join(
          " ", Results.Select(s => s.ErrorMessage).ToArray());
      }
      return results;
    }
  }
}