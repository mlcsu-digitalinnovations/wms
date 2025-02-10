using System;
using System.Globalization;
using WmsHub.Business.Enums;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Models
{
  public class ReferralSearch
  {
    public enum Variation
    { 
      Original = 0,
      Upper,
      Lower,
      Variant
    }


    private string _telephoneNumber;
    private string _mobileNumber;
    private static TextInfo textInfo = new CultureInfo("en-GB", false).TextInfo;

    public string TelephoneNumber
    {
      get => _telephoneNumber;
      set
      {
        try
        {
          _telephoneNumber = value.ConvertToUkLandlineNumber(
            allowNullOrWhiteSpace: true);
        }
        catch (FormatException)
        {
          _telephoneNumber = null;
        }
      }
    }
    public string MobileNumber
    {
      get => _mobileNumber;
      set
      {
        try
        {
          _mobileNumber = value.ConvertToUkMobileNumber(
            allowNullOrWhiteSpace: true);
        }
        catch (FormatException)
        {
          _mobileNumber = null;
        }
      }
    }
    public string EmailAddress { get; set; }
    public string Ubrn { get; set; }
    public string NhsNumber { get; set; }
    public string FamilyName { get; set; }
    public string GivenName { get; set; }
    public string Postcode { get; set; }
    public bool? IsVulnerable { get; set; }
    public string[] Statuses { get; set; }
    public int? Limit { get; set; }
    public string ReferralSource { get; set; }
    public SearchFilter DelayedReferralsFilter { get; set; }

    public bool HasSearchParameters => 
      Ubrn != null || 
      FamilyName != null || 
      TelephoneNumber != null ||
      Postcode != null ||
      NhsNumber != null ||
      GivenName != null ||
      MobileNumber != null ||
      EmailAddress != null;
    
    public string[] GetFamilyNameVariations()
    {
      return GetVariations(FamilyName);
    }

    public string[] GetGivenNameVariations()
    {
      return GetVariations(GivenName);
    }

    public string[] GetEmailAddressVariations()
    {
      return GetVariations(EmailAddress);
    }

    public string[] GetPostcodeVariations()
    {
      if (string.IsNullOrWhiteSpace(Postcode))
      {
        return null;
      }
      else
      {
        return new string[]
        {
          Postcode,
          Postcode.ToUpper(),
          Postcode.ToLower(),
          Postcode.ConvertToNoSpaceUpperPostcode()
        };
      }
    }

    public string[] GetVariations(string property)
    {
      if (string.IsNullOrWhiteSpace(property))
      {
        return null;
      }
      else
      {
        return new string[]
        {
          property,
          property.ToUpper(),
          property.ToLower(),
          textInfo.ToTitleCase(property.ToLower())
        };
      }
    }

  }
}