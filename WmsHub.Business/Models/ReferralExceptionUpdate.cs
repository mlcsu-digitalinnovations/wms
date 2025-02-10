using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Enums;
using WmsHub.Common.Attributes;

namespace WmsHub.Business.Models
{
  public class ReferralExceptionUpdate : ReferralCreateBase,
    IReferralExceptionUpdate, IValidatableObject
  {
    public CreateReferralException ExceptionType { get; set; }
    public DateTimeOffset? MostRecentAttachmentDate { get; set; }
    [NhsNumber(allowNulls: true)]
    public string NhsNumberWorkList { get; set; }
    [NhsNumber(allowNulls: true)]
    public string NhsNumberAttachment { get; set; }
    public string ReferralAttachmentId { get; set; }

    public IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
      if (ExceptionType != CreateReferralException.MissingAttachment)
      {
        if (string.IsNullOrWhiteSpace(ReferralAttachmentId))
        {
          yield return new ValidationResult(
            $"The {nameof(ReferralAttachmentId)} field is required when the exception type is " +
              $"{ExceptionType}.");
        }
        if ((MostRecentAttachmentDate ?? default) == default)
        {
          yield return new ValidationResult(
            $"The {nameof(MostRecentAttachmentDate)} field is required when the exception type " +
              $"is {ExceptionType}.");
        }
      }

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

    public ReferralExceptionUpdate()
    {
      ReferralSource = $"{Enums.ReferralSource.GpReferral}";
    }
  }
}