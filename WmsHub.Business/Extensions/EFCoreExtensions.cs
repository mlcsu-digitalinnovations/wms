using AngleSharp.Text;
using System;
using System.Text.RegularExpressions;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Common.Extensions;

namespace WmsHub.Business.Extensions;

public static class EFCoreExtensions
{
  public static void SetReferralStatusFailedToContactAndUpdate(
    this Referral entity,
    Guid userId)
  {
    entity.ModifiedByUserId = userId;
    entity.ModifiedAt = DateTimeOffset.Now;

    if (!string.IsNullOrWhiteSpace(entity.Mobile)
        && entity.Mobile.IsUkMobile())
    {
      entity.Status = ReferralStatus.FailedToContactTextMessage.ToString();
    }
    else if (!string.IsNullOrWhiteSpace(entity.Email)
        && IsValidEmail(entity.Email))
    {
      entity.Status = ReferralStatus.FailedToContactEmailMessage.ToString();
    }
    else
    {
      string message = "";
      if (string.IsNullOrWhiteSpace(entity.Mobile))
      {
        message = "Mobile is not set.  ";
      }

      if (!entity.Mobile.IsUkMobile())
      {
        message = "Mobile is not valid.  ";
      }

      if (string.IsNullOrWhiteSpace(entity.Email))
      {
        message += "Email is not set.  ";
      }

      if (IsValidEmail(entity.Email))
      {
        message += "Email address is not valid.";
      }

      entity.Status = ReferralStatus.Exception.ToString();
      entity.StatusReason = message;
    }
  }

  public static void SetReferralStatusAndUpdateForSms(
    this Referral entity,
    ReferralStatus status,
    Guid userId)
  {
    entity.Status = status.ToString();
    entity.ModifiedByUserId = userId;
    entity.ModifiedAt = DateTimeOffset.Now;
    if (string.IsNullOrWhiteSpace(entity.Mobile) ||
      !entity.Mobile.IsUkMobile())
    {
      entity.Status = ReferralStatus.Exception.ToString();
      entity.StatusReason = "Mobile number is not valid.";
    }
  }

  public static void SetReferralStatusAndUpdateForEmail(
   this Referral entity,
   ReferralStatus status,
   Guid userId)
  {
    entity.Status = status.ToString();
    entity.ModifiedByUserId = userId;
    entity.ModifiedAt = DateTimeOffset.Now;
    if (string.IsNullOrWhiteSpace(entity.Email) ||
      !IsValidEmail(entity.Email))
    {
      entity.Status = ReferralStatus.Exception.ToString();
      entity.StatusReason = "Email address is not valid.";
    }
  }

  public static void SetRecievedAndOutcome(
    this TextMessage entity, 
    Guid userId, 
    string outcome = "Sent")
  {
    entity.Received = DateTime.UtcNow;
    entity.Outcome = outcome;
    entity.ModifiedByUserId = userId;
    entity.ModifiedAt = DateTimeOffset.Now;
  }

  private static bool IsValidEmail(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      return false;
    }

    return Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
  }
}
