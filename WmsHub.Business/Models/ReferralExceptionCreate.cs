using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;

namespace WmsHub.Business.Models
{
  public class ReferralExceptionCreate 
    : ReferralCreateBase, IReferralExceptionCreate, IValidatableObject
  { 
    public CreateReferralException ExceptionType { get; set; }
    [NhsNumber(allowNulls: true)]
    public string NhsNumberWorkList { get; set; }
    [NhsNumber(allowNulls: true)]
    public string NhsNumberAttachment { get; set; }
    public string ServiceId { get; set; }
    public Common.Enums.SourceSystem? SourceSystem { get; set; }
    public decimal? DocumentVersion { get; set; }
    public IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      if (ExceptionType == CreateReferralException.NhsNumberMismatch)
      {
        if (string.IsNullOrWhiteSpace(NhsNumberWorkList))
        {
          yield return new ValidationResult(
            $"The {nameof(NhsNumberWorkList)} field cannot be null or " +
            $"white space.");
        }
        if (string.IsNullOrWhiteSpace(NhsNumberAttachment))
        {
          yield return new ValidationResult(
            $"The {nameof(NhsNumberAttachment)} field cannot be null or " +
            $"white space.");
        }
        if (!string.IsNullOrWhiteSpace(NhsNumberAttachment) &&
          !string.IsNullOrWhiteSpace(NhsNumberWorkList) &&
          NhsNumberAttachment == NhsNumberWorkList)
        {
          yield return new ValidationResult(
            $"The {nameof(NhsNumberAttachment)} field must NOT match the " +
            $"{nameof(NhsNumberWorkList)} field for an exception type of " +
            $"{ExceptionType}.");
        }
      }
    }
    public string ReferralSource { get; set; } 

    public ReferralExceptionCreate ()
    {
      ReferralSource = $"{Enums.ReferralSource.GpReferral}";
    }
  }
}

