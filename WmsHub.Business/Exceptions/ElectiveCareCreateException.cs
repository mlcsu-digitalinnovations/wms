using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WmsHub.Business.Exceptions;

[Serializable]
public class ElectiveCareCreateException : AReferralValidationException
{

  public ElectiveCareCreateException()
    : base()
  { }

  public ElectiveCareCreateException(List<ValidationResult> results)
    : base(results)
  { }

  public ElectiveCareCreateException(
    string message, List<ValidationResult> results)
    : base(results, message)
  { }

  public ElectiveCareCreateException(
    string message, Exception inner, List<ValidationResult> results)
    : base(results, message, inner)
  { }
}
