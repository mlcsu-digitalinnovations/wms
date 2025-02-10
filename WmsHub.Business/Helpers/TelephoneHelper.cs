using System;
using System.Text.RegularExpressions;
using WmsHub.Business.Models;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;

namespace WmsHub.Business.Helpers;

public static class PhoneHelper
{
  /// <summary>
  /// Converts the mobile and telephone properties of the interface to valid
  /// UK phone numbers if possible, swaps the properties if telephone is a 
  /// mobile number and vice versa. Then sets the IsMobileValid and 
  /// IsTelephoneValid properties if their associated properties are valid.
  /// </summary>
  public static void FixPhoneNumberFields(
    this IPhoneBase phone)
  {
    Regex regexMobile = new(Constants.REGEX_MOBILE_PHONE_UK);
    Regex regexTelephone = new(Constants.REGEX_LANDLINE_HOME_UK);

    // Sanitize numbers to UK.
    if (string.IsNullOrWhiteSpace(phone.Mobile))
    {
      phone.Mobile = "";
    }
    try
    {
      phone.Mobile = phone.Mobile.ConvertToUkPhoneNumber(true);
    }
    catch (FormatException)
    {
      // Ignore exception if format is incorrect because we want to keep the
      // invalid mobile number.
    }

    if (string.IsNullOrWhiteSpace(phone.Telephone))
    {
      phone.Telephone = "";
    }
    try
    {
      phone.Telephone = phone.Telephone.ConvertToUkPhoneNumber(true);
    }
    catch (FormatException)
    {
      // Ignore exception if format is incorrect because we want to keep the
      // invalid telephone number.
    }

    // Is Mobile Mobile?
    if (regexMobile.IsMatch(phone.Mobile))
    {
      // Mobile is Mobile.
      phone.IsMobileValid = true;
    }
    else
    {
      // Mobile is NOT Mobile.
      // Is Telephone Mobile?
      if (regexMobile.IsMatch(phone.Telephone))
      {
        // Mobile is NOT Mobile, Telephone is Mobile.
        // Is Mobile Telephone?
        if (regexTelephone.IsMatch(phone.Mobile))
        {
          // Mobile is Telephone, Telephone is Mobile.
          // Switch Mobile and Telephone.
          (phone.Mobile, phone.Telephone) = (phone.Telephone, phone.Mobile);
          phone.IsMobileValid = true;
          phone.IsTelephoneValid = true;
        }
        else
        {
          // Mobile is NOT Telephone, Telephone is Mobile.
          // Move Telephone to Mobile.
          phone.Mobile = phone.Telephone;
          phone.IsMobileValid = true;
          phone.Telephone = null;
          phone.IsTelephoneValid = false;
        }
      }
      else
      {
        // Mobile is NOT Mobile, Telephone is NOT Mobile.
        // Is Telephone Telephone?
        if (regexTelephone.IsMatch(phone.Telephone))
        {
          // Mobile is NOT Mobile, Telephone is Telephone.
          phone.IsTelephoneValid = true;
        }
        else
        {
          // Telephone is NOT Telephone.
          // Is Mobile Telephone?
          if (regexTelephone.IsMatch(phone.Mobile))
          {
            // Mobile is Telephone, Telephone is NOT Telephone.
            // Move Mobile to Telephone.
            phone.Telephone = phone.Mobile;
            phone.IsTelephoneValid = true;
            phone.Mobile = null;
          }
          else
          {
            // Mobile is NOT Telephone, Telephone is NOT Telephone.
            phone.IsTelephoneValid = false;
          }
        }

        phone.IsMobileValid = false;
      }
    }

    // Mobile update complete, check Telephone.
    // Is Telephone Telephone?
    if (phone.Telephone != null && regexTelephone.IsMatch(phone.Telephone))
    {
      // Telephone is Telephone.
      // Is Mobile Telephone.
      if (phone.Mobile != null && regexTelephone.IsMatch(phone.Mobile))
      {
        // Mobile is Telephone, Telephone is Telephone.
        phone.Mobile = null;
      }
      phone.IsTelephoneValid = true;
    }
    else
    {
      // Telephone is NOT Telephone.
      // Is Telephone Mobile?
      if (phone.Telephone != null 
        && regexMobile.IsMatch(phone.Telephone)
        && phone.IsMobileValid == true)
      {
        // Mobile is Mobile, Telephone is Mobile.
        phone.Telephone = null;
      }
      phone.IsTelephoneValid = false;
    }

    phone.Mobile = phone.Mobile == "" ? null : phone.Mobile;
    phone.Telephone = phone.Telephone == "" ? null : phone.Telephone;
  }
}