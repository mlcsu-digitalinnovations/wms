using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WmsHub.Business.Helpers;
using WmsHub.Common.Attributes;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Common.Validation;

namespace WmsHub.Business.Models
{
  public class ReferralCreate
    : ReferralCreateBase, IReferralCreate, IValidatableObject
  {
    private string _email;
    private string _mobile;
    private string _referringGpPracticeNumber;
    private string _sex;
    private string _telephone;

    [Required]
    [NhsNumber]
    public string NhsNumber { get; set; }
    [Required]
    public DateTimeOffset? DateOfReferral { get; set; }
    [Required]
    public string ReferringGpPracticeNumber
    {
      get => _referringGpPracticeNumber;
      set
      {
        if (string.IsNullOrWhiteSpace(value))
        {
          _referringGpPracticeNumber = Constants.UNKNOWN_GP_PRACTICE_NUMBER;
          return;
        }
        string[] values = value.Split(' ', StringSplitOptions.TrimEntries);
        foreach (string code in values)
        {
          if (RegexUtilities.IsValidGpPracticeOdsCode(code))
          {
            _referringGpPracticeNumber = code.ToUpper();
            return;
          }
        }
        _referringGpPracticeNumber = Constants.UNKNOWN_GP_PRACTICE_NUMBER;
      }
    }
    [Required]
    [MaxLength(200)]
    public string FamilyName { get; set; }
    [Required]
    [MaxLength(200)]
    public string GivenName { get; set; }
    [MaxLength(200)]
    public string Address1 { get; set; }
    [MaxLength(200)]
    public string Address2 { get; set; }
    [MaxLength(200)]
    public string Address3 { get; set; }
    private string _postcode;
    [Required(ErrorMessage = "The Postcode field is not a valid postcode.")]
    public string Postcode
    {
      get => _postcode;
      set => _postcode = value.ConvertToPostcode();
    }

    public bool? IsTelephoneValid { get; private set; }
    [MaxLength(200)]
    public string Telephone
    {
      get => _telephone;
      set
      {
        string telephone;
        try
        {
          // only converting to phone number so validation can switch
          // telephone and mobile property values if required
          telephone = value.ConvertToUkPhoneNumber(true);
        }
        catch (FormatException)
        {
          telephone = null;
        }
        if (string.IsNullOrWhiteSpace(telephone))
        {
          IsTelephoneValid = false;
          _telephone = null;
        }
        else
        {
          IsTelephoneValid = null;
          _telephone = telephone;
        }
      }
    }

    public bool? IsMobileValid { get; private set; }
    [MaxLength(200)]
    public string Mobile
    {
      get => _mobile;
      set
      {
        string mobile;
        try
        {
          // only converting to phone number so validation can switch
          // telephone and mobile property values if required
          mobile = value.ConvertToUkPhoneNumber(true);
        }
        catch (FormatException)
        {
          mobile = null;
        }

        if (string.IsNullOrWhiteSpace(mobile))
        {
          IsMobileValid = false;
          _mobile = null;
        }
        else
        {
          IsMobileValid = null;
          _mobile = mobile;
        }
      }
    }

    [MaxLength(200)]
    public string Email
    {
      get => _email;
      set
      {
        _email = value.EmailCleaner(Constants.INVALID_EMAIL_TERMS);
        if (!RegexUtilities.IsValidEmail(_email))
        {
          _email = string.Empty;
        }
      }
    }
    [Required]
    [AgeRange(Constants.MIN_GP_REFERRAL_AGE, Constants.MAX_GP_REFERRAL_AGE)]
    public DateTimeOffset? DateOfBirth { get; set; }

    [Required]
    public string Sex { get => _sex; set => _sex = value.TryParseSex(); }
    public bool? IsVulnerable { get; set; }
    public string VulnerableDescription { get; set; }
    [MaxLength(200)]
    public string Ethnicity { get; set; }
    public bool? HasAPhysicalDisability { get; set; }
    public bool? HasALearningDisability { get; set; }
    public bool? HasRegisteredSeriousMentalIllness { get; set; }
    [Required]
    public bool? HasHypertension { get; set; }
    [Required]
    public bool? HasDiabetesType1 { get; set; }
    [Required]
    public bool? HasDiabetesType2 { get; set; }
    [Required, Range(Constants.MIN_HEIGHT_CM, Constants.MAX_HEIGHT_CM)]
    public decimal? HeightCm { get; set; }
    [Required, Range(Constants.MIN_WEIGHT_KG, Constants.MAX_WEIGHT_KG)]
    public decimal? WeightKg { get; set; }

    public string CriDocument { get; set; }
    public DateTimeOffset? CriLastUpdated { get; set; }

    [Required, Range(27.5, 90)]
    public decimal? CalculatedBmiAtRegistration { get; set; }
    [Required]
    public DateTimeOffset? DateOfBmiAtRegistration { get; set; }
    [Required]
    public string ReferringGpPracticeName { get; set; }

    [Required]
    [Range(0, long.MaxValue)]
    public long? ReferralAttachmentId { get; set; }

    [Required]
    [Range(0, long.MaxValue)]
    public long? MostRecentAttachmentId { get; set; }

    public decimal? DocumentVersion { get; set; }
    public Common.Enums.SourceSystem? SourceSystem { get; set; }
    public string ServiceId { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {

      if (!string.IsNullOrWhiteSpace(CriDocument) && CriLastUpdated == null)
      {
        yield return new ValidationResult("The field " +
        $"{nameof(CriLastUpdated)} cannot be null or empty if the " +
        $"{nameof(CriDocument)} property is true.");
      }

      if (string.IsNullOrWhiteSpace(Telephone) &&
   string.IsNullOrWhiteSpace(Mobile))
      {
        yield return new ValidationResult("One of the fields: " +
          $"{nameof(Telephone)} or {nameof(Mobile)} is required.");
      }
      else
      {

        // if the mobile number is null or empty or not a mobile number
        // but the telephone is a mobile number then switch their values
        if ((string.IsNullOrWhiteSpace(Mobile) || !Mobile.IsUkMobile())
        && Telephone.IsUkMobile())
        {
          string tempNumber = Mobile;
          Mobile = Telephone;
          Telephone = tempNumber;
        }
        // if the telephone number is null or empty or a mobile number
        // but the mobile not a mobile number then switch their values
        else if ((string.IsNullOrWhiteSpace(Telephone) || Telephone.IsUkMobile())
          && !Mobile.IsUkMobile())
        {
          string tempNumber = Mobile;
          Mobile = Telephone;
          Telephone = tempNumber;
        }
        // if the mobile and telephone are both mobiles 
        else if (Mobile.IsUkMobile() && Telephone.IsUkMobile())
        {
          Telephone = null;
        }

      }


      if ((HasDiabetesType1 ?? false) == false &&
        (HasDiabetesType2 ?? false) == false &&
        (HasHypertension ?? false) == false)
      {
        yield return new ValidationResult("A diagnosis of Diabetes Type 1 or " +
          "Diabetes Type 2 or Hypertension is required.");
      }

      if (!string.IsNullOrWhiteSpace(Ethnicity))
      {
        if (!Ethnicity.TryParseToEnumName<Enums.Ethnicity>(
          out var resultEthnicity))
        {
          yield return
            new InvalidValidationResult(nameof(Ethnicity), Ethnicity);
        }

      }

      if (!Sex.TryParseToEnumName<Enums.Sex>(out var resultSex))
      {
        yield return new InvalidValidationResult(nameof(Sex), Sex);
      }
    }
  }
}
