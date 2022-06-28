using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Business.Enums;
using WmsHub.Common.Helpers;
using WmsHub.Common.Extensions;

namespace WmsHub.Ui.Models
{
  public class ReferralListItemModel
  {
    public Guid Id { get; set; }
    public string Ubrn { get; set; }
    public string NhsNumber { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public int DateOfBirthDay => DateOfBirth.Day;
    public int DateOfBirthMonth => DateOfBirth.Month;
    public int DateOfBirthYear => DateOfBirth.Year;


    public DateTimeOffset DateOfReferral { get; set; }
    private string _status;
    public string Status { get => _status; set => _status = value; }
    public string ListDisplayStatus
    {
      get
      {
        if (_status == ReferralStatus.RmcCall.ToString() && HasDelayReason)
        {
          return $"{_status} (Delayed)";
        }
        return _status;
      }
      set => _status = value;
    }
    public string StatusReason { get; set; }
    public string Priority { get; set; }
    public bool ConsentForFutureContactForEvaluation { get; set; }
    public string SelectedEthnicity { get; set; }
    public List<SelectListItem> EthnicityList { get; set; }
    public Guid ProviderId { get; set; }
    public bool DelayReferral { get; set; }
    public DateTimeOffset? DelayUntil { get; set; }
    public string DelayReason { get; set; }
    public int NumberOfDelays { get; set; }
    public string SelectedDelayReferralLength { get; set; }
    public bool CallFailed { get; set; }
    public DateTimeOffset? DelayFrom { get; set; }
    public decimal Bmi { get; set; }
    public decimal SelectedEthnicGroupMinimumBmi { get; set; }
    public bool IsBmiTooLow { get; set; }

    public string Telephone { get; set; }
    [RegularExpression(Constants.REGEX_MOBILE_PHONE_UK)]
    public string Mobile { get; set; }
    [RegularExpression(Constants.REGEX_EMAIL_ADDRESS)]
    public string Email { get; set; }
    public string EmailReadOnly => Email;
    public string ReferringGpPracticeName { get; set; }

    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string Postcode { get; set; }

    [HiddenInput]
    public bool IsVulnerable { get; set; }
    public string VulnerableDescription { get; set; }

    [HiddenInput]
    public bool IsException { get; set; }
    public string ExceptionDetails { get; set; }
    public string ExceptionStatus { get; set; }
    public string RejectionReason => IsException ? StatusReason : "";

    public string ActiveUser { get; set; }

    public List<Provider> Providers { get; set; }

    public DateTimeOffset DisplayDateOfBirth { get { return DateOfBirth; } }
    public DateTimeOffset DisplayDateOfReferral
    { get { return DateOfReferral; } }

    public bool Export { get; set; }

    public IEnumerable<ReferralAuditListItemModel> AuditList { get; set; } =
      new List<ReferralAuditListItemModel>();
    public bool HasAuditList => AuditList?.Any() ?? false;

    public bool HasDelayReason => !string.IsNullOrWhiteSpace(DelayReason);
    public bool HasEmail => !string.IsNullOrWhiteSpace(Email);
    public bool HasProviders => Providers?.Count > 0;
    public bool HasStatusReason => !string.IsNullOrWhiteSpace(StatusReason);

    public string ReferralSource { get; set; }
    public string ReferringPharmacyODSCode { get; set; }
    public string ReferringPharmacyEmail { get; set; }

    public string ReferringPharmacy =>
      $"{ReferringPharmacyODSCode} ({ReferringPharmacyEmail})";

    public bool IsPharmacyReferral
    {
      get
      {
        if (ReferralSource.TryParseToEnumName(out ReferralSource parsedEnum))
        {
          if (parsedEnum == Business.Enums.ReferralSource.Pharmacy)
          {
            return true;
          }
        }
        return false;
      }
    }

    private string _providerName;


    public string ProviderName
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(_providerName))
          return _providerName;

        if (Providers == null || ProviderId == Guid.Empty)
          return "A provider";

        Provider selectedProvider =
          Providers.SingleOrDefault(p => p.Id == ProviderId);

        if (selectedProvider == null)
          return _providerName;

        return selectedProvider.Name;
      }
      set => _providerName = value;
    }

    public bool IsProviderPreviouslySelected { get; set; }

    public bool IsProviderSelected => ProviderId != Guid.Empty;

    private static readonly string[] _canConfirmEmailStatuses = new[]
    {
      ReferralStatus.New.ToString(),
      ReferralStatus.TextMessage1.ToString(),
      ReferralStatus.TextMessage2.ToString(),
      ReferralStatus.ChatBotCall1.ToString(),
      ReferralStatus.ChatBotTransfer.ToString(),
      ReferralStatus.ChatBotCall2.ToString(),
      ReferralStatus.RmcCall.ToString(),
    };

    public bool CanConfirmEmail => _canConfirmEmailStatuses.Contains(Status);

    private static readonly string[] _canConfirmEthnicityStatuses = new[]
    {
      ReferralStatus.New.ToString(),
      ReferralStatus.TextMessage1.ToString(),
      ReferralStatus.TextMessage2.ToString(),
      ReferralStatus.ChatBotCall1.ToString(),
      ReferralStatus.ChatBotTransfer.ToString(),
      ReferralStatus.ChatBotCall2.ToString(),
      ReferralStatus.RmcCall.ToString(),
    };

    public bool CanConfirmEthnicity =>
      _canConfirmEthnicityStatuses.Contains(Status);

    private static readonly string[] _canShowProvidersStatuses = new[]
    {
      ReferralStatus.New.ToString(),
      ReferralStatus.TextMessage1.ToString(),
      ReferralStatus.TextMessage2.ToString(),
      ReferralStatus.ChatBotCall1.ToString(),
      ReferralStatus.ChatBotTransfer.ToString(),
      ReferralStatus.ChatBotCall2.ToString(),
      ReferralStatus.RmcCall.ToString(),
    };

    public bool CanShowProviders =>
      _canShowProvidersStatuses.Contains(Status);

    private static readonly string[] _canDelayStatuses = new[]
    {
      ReferralStatus.New.ToString(),
      ReferralStatus.TextMessage1.ToString(),
      ReferralStatus.TextMessage2.ToString(),
      ReferralStatus.ChatBotCall1.ToString(),
      ReferralStatus.ChatBotTransfer.ToString(),
      ReferralStatus.ChatBotCall2.ToString(),
      ReferralStatus.RmcCall.ToString(),
    };

    public bool CanDelay => _canDelayStatuses.Contains(Status) && !IsBmiTooLow;

    private static readonly string[] _canUnableToContactStatuses = new[]
    {
      ReferralStatus.RmcCall.ToString()
    };

    public bool CanUnableToContact =>
      _canUnableToContactStatuses.Contains(Status) && !IsBmiTooLow;

    private static readonly string[] _canRejectToEreferralsStatuses = new[]
    {
      ReferralStatus.Exception.ToString(),
      ReferralStatus.New.ToString(),
      ReferralStatus.TextMessage1.ToString(),
      ReferralStatus.TextMessage2.ToString(),
      ReferralStatus.ChatBotCall1.ToString(),
      ReferralStatus.ChatBotTransfer.ToString(),
      ReferralStatus.ChatBotCall2.ToString(),
      ReferralStatus.RmcCall.ToString(),
      ReferralStatus.ProviderDeclinedByServiceUser.ToString(),
      ReferralStatus.ProviderRejected.ToString(),
      ReferralStatus.ProviderTerminated.ToString(),
      ReferralStatus.FailedToContact.ToString()
    };

    private bool IsGpReferral
    {
      get
      {
        if (ReferralSource.TryParseToEnumName(out ReferralSource parsedEnum))
        {
          if (parsedEnum == Business.Enums.ReferralSource.GpReferral)
          {
            return true;
          }
        }
        return false;
      }
    }

    public bool CanRejectToEreferrals =>
      _canRejectToEreferralsStatuses.Contains(Status) && IsGpReferral;

    private static readonly string[] _canAddToCallListStatuses = new[]
    {
      ReferralStatus.RmcDelayed.ToString(),
      ReferralStatus.Letter.ToString(),
      ReferralStatus.ProviderDeclinedByServiceUser.ToString(),
      ReferralStatus.ProviderRejected.ToString(),
      ReferralStatus.ProviderTerminated.ToString(),
      ReferralStatus.LetterSent.ToString(),
    };

    public bool CanAddToCallList =>
      _canAddToCallListStatuses.Contains(Status) && !IsBmiTooLow;

    public int MinGpReferralAge
    {
      get
      {
        return Constants.MIN_GP_REFERRAL_AGE;
      }
    }

    public int MaxGpReferralAge
    {
      get
      {
        return Constants.MAX_GP_REFERRAL_AGE;
      }
    }
  }
}