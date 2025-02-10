using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.ReferralStatusReason;
using WmsHub.Common.Extensions;
using WmsHub.Common.Helpers;
using WmsHub.Ui.Models.Extensions;

namespace WmsHub.Ui.Models;

public class ReferralListItemModel
{
  private static readonly string[] s_canAddToCallListStatuses =
  [
    ReferralStatus.RmcDelayed.ToString(),
    ReferralStatus.Letter.ToString(),
    ReferralStatus.ProviderDeclinedByServiceUser.ToString(),
    ReferralStatus.ProviderRejected.ToString(),
    ReferralStatus.ProviderTerminated.ToString(),
    ReferralStatus.LetterSent.ToString(),
  ];
  private static readonly string[] s_canConfirmEmailStatuses =
  [
    ReferralStatus.New.ToString(),
    ReferralStatus.TextMessage1.ToString(),
    ReferralStatus.TextMessage2.ToString(),
    ReferralStatus.ChatBotCall1.ToString(),
    ReferralStatus.ChatBotTransfer.ToString(),
    ReferralStatus.RmcCall.ToString(),
    ReferralStatus.TextMessage3.ToString()
  ];
  private static readonly string[] s_canConfirmEthnicityStatuses =
  [
    ReferralStatus.New.ToString(),
    ReferralStatus.TextMessage1.ToString(),
    ReferralStatus.TextMessage2.ToString(),
    ReferralStatus.ChatBotCall1.ToString(),
    ReferralStatus.ChatBotTransfer.ToString(),    
    ReferralStatus.RmcCall.ToString(),
    ReferralStatus.TextMessage3.ToString()
  ];
  private static readonly string[] s_canDelayStatuses =
  [
    ReferralStatus.New.ToString(),
    ReferralStatus.TextMessage1.ToString(),
    ReferralStatus.TextMessage2.ToString(),
    ReferralStatus.ChatBotCall1.ToString(),
    ReferralStatus.ChatBotTransfer.ToString(),
    ReferralStatus.RmcCall.ToString()
  ];
  private static readonly string[] s_canRejectToEreferralsStatuses =
  [
    ReferralStatus.Exception.ToString(),
  ];
  private static readonly string[] s_canRejectAfterProviderSelection =
  [
    ReferralStatus.ProviderDeclinedByServiceUser.ToString(),
    ReferralStatus.ProviderRejected.ToString(),
    ReferralStatus.ProviderTerminated.ToString(),
  ];
  private static readonly string[] s_canRejectBeforeProviderSelection =
  [
    ReferralStatus.New.ToString(),
    ReferralStatus.TextMessage1.ToString(),
    ReferralStatus.TextMessage2.ToString(),
    ReferralStatus.ChatBotCall1.ToString(),
    ReferralStatus.ChatBotTransfer.ToString(),
    ReferralStatus.RmcCall.ToString(),
    ReferralStatus.RmcDelayed.ToString(),
    ReferralStatus.TextMessage3.ToString()
  ];
  private static readonly string[] s_canShowProvidersStatuses =
  [
    ReferralStatus.New.ToString(),
    ReferralStatus.TextMessage1.ToString(),
    ReferralStatus.TextMessage2.ToString(),
    ReferralStatus.ChatBotCall1.ToString(),
    ReferralStatus.ChatBotTransfer.ToString(),
    ReferralStatus.RmcCall.ToString(),
    ReferralStatus.TextMessage3.ToString()
  ];
  private static readonly string[] s_canSendElectiveCareLink =
  [
    ReferralStatus.New.ToString(),
    ReferralStatus.TextMessage1.ToString(),
    ReferralStatus.TextMessage2.ToString(),
    ReferralStatus.ChatBotCall1.ToString(),
    ReferralStatus.ChatBotTransfer.ToString(),
    ReferralStatus.RmcCall.ToString(),
    ReferralStatus.TextMessage3.ToString()
  ];
  private static readonly string[] s_canUnableToContactStatuses =
  [
    ReferralStatus.RmcCall.ToString()
  ];
  private string _providerName;
  private ReferralStatusReason[] _rejectionReasons;

  public string ActiveUser { get; set; }
  public string Address1 { get; set; }
  public string Address2 { get; set; }
  public string Address3 { get; set; }
  public IEnumerable<ReferralAuditListItemModel> AuditList { get; set; } =
    new List<ReferralAuditListItemModel>();
  public decimal Bmi { get; set; }
  public bool CallFailed { get; set; }
  public bool CanAddToCallList =>
    s_canAddToCallListStatuses.Contains(Status) && !IsBmiTooLow;
  public bool CanConfirmEmail => s_canConfirmEmailStatuses.Contains(Status);
  public bool CanConfirmEthnicity =>
    s_canConfirmEthnicityStatuses.Contains(Status);
  public bool CanDelay => s_canDelayStatuses.Contains(Status) && !IsBmiTooLow && MaxDaysToDelay > 0;
  public bool CanOverrideException => this.GetCanOverrideException();
  public bool CanRejectAfterProviderSelection =>
    s_canRejectAfterProviderSelection.Contains(Status) && IsGpReferral;
  public bool CanRejectBeforeProviderSelection =>
    s_canRejectBeforeProviderSelection.Contains(Status) && IsGpReferral;
  public bool CanRejectToEreferrals =>
    s_canRejectToEreferralsStatuses.Contains(Status) && IsGpReferral;
  public bool CanSendElectiveCareLink => 
    s_canSendElectiveCareLink.Contains(Status) && IsElectiveCareReferral && HasEmail;
  public bool CanShowProviders => s_canShowProvidersStatuses.Contains(Status);
  public bool CanUnableToContact =>
    s_canUnableToContactStatuses.Contains(Status) && !IsBmiTooLow;
  public bool ConsentForFutureContactForEvaluation { get; set; }
  public DateTimeOffset DateOfBirth { get; set; }
  public int DateOfBirthDay => DateOfBirth.Day;
  public int DateOfBirthMonth => DateOfBirth.Month;
  public int DateOfBirthYear => DateOfBirth.Year;
  public DateTimeOffset DateOfReferral { get; set; }
  public DateTimeOffset? DelayFrom { get; set; }
  public string DelayReason { get; set; }
  public bool DelayReferral { get; set; }
  public DateTimeOffset? DelayUntil { get; set; }
  public DateTimeOffset DisplayDateOfBirth => DateOfBirth;
  public DateTimeOffset DisplayDateOfReferral => DateOfReferral;
  [RegularExpression(Constants.REGEX_EMAIL_ADDRESS)]
  public string Email { get; set; }
  public string EmailReadOnly => Email;
  public string ExceptionDetails { get; set; }
  public string ExceptionStatus { get; set; }
  public bool Export { get; set; }
  public string FamilyName { get; set; }
  public string GivenName { get; set; }
  public ReferralAuditListGroupModel GroupedAuditList { get; set; }
  public bool HasAuditList => AuditList?.Any() ?? false;
  public bool HasAuditGroupList => GroupedAuditList?.PastItems?.Any() ?? false;
  public bool HasDelayReason => !string.IsNullOrWhiteSpace(DelayReason);
  public bool HasEmail => !string.IsNullOrWhiteSpace(Email);
  public bool HasProviders => Providers?.Count > 0;
  public bool HasServiceUserEthnicityAndServiceUserEthnicityGroup =>
    SelectedServiceUserEthnicity != null && SelectedServiceUserEthnicityGroup != null;
  public bool HasStatusReason => !string.IsNullOrWhiteSpace(StatusReason);
  public Guid Id { get; set; }
  public bool IsBmiTooLow { get; set; }
  public bool IsElectiveCareReferral => 
    ReferralSource.TryParseToEnumName(out ReferralSource parsedEnum)
    && parsedEnum == Business.Enums.ReferralSource.ElectiveCare;
  [HiddenInput]
  public bool IsException { get; set; }
  public bool IsPharmacyReferral
  {
    get => ReferralSource.TryParseToEnumName(out ReferralSource parsedEnum)
      && parsedEnum == Business.Enums.ReferralSource.Pharmacy;
  }
  public bool IsProviderPreviouslySelected { get; set; }
  public bool IsProviderSelected => ProviderId != Guid.Empty;
  [HiddenInput]
  public bool IsVulnerable { get; set; }
  public string ListDisplayStatus
  {
    get => Status == ReferralStatus.RmcCall.ToString() && HasDelayReason
      ? $"{Status} (Delayed)"
      : Status;
    set => Status = value;
  }
  public DateTimeOffset? MaxDateToDelay { get; set; }
  public int MaxDaysToDelay { get; set; }
  public int MaxGpReferralAge { get => Constants.MAX_GP_REFERRAL_AGE; }
  public int MinGpReferralAge { get => Constants.MIN_GP_REFERRAL_AGE; }
  [RegularExpression(Constants.REGEX_MOBILE_PHONE_UK)]
  public string Mobile { get; set; }
  public string NhsNumber { get; set; }
  public int NumberOfDelays { get; set; }
  public string Postcode { get; set; }
  public string Priority { get; set; }
  public List<Provider> Providers { get; set; }
  public Guid ProviderId { get; set; }
  public string ProviderName
  {
    get
    {
      if (!string.IsNullOrWhiteSpace(_providerName))
      {
        return _providerName;
      }

      if (Providers == null || ProviderId == Guid.Empty)
      {
        return "A provider";
      }

      Provider selectedProvider = Providers
        .SingleOrDefault(p => p.Id == ProviderId);

      return selectedProvider == null ? _providerName : selectedProvider.Name;
    }
    set => _providerName = value;
  }
  public string ProviderUbrn { get; set; }
  public string ReferralSource { get; set; }
  public string ReferringGpPracticeName { get; set; }
  public string ReferringPharmacy =>
    $"{ReferringPharmacyOdsCode} ({ReferringPharmacyEmail})";
  public string ReferringPharmacyEmail { get; set; }
  public string ReferringPharmacyOdsCode { get; set; }
  public ReferralStatusReason[] ReferralStatusReasons {
    get => _rejectionReasons ?? Array.Empty<ReferralStatusReason>(); 
    set => _rejectionReasons = value; 
  }
  public string RejectionReason =>
    (IsException || IsBmiTooLow) ? StatusReason : "";
  public string SelectedDelayReferralLength { get; set; }
  public decimal SelectedEthnicGroupMinimumBmi { get; set; }
  public string SelectedEthnicity { get; set; }
  public string SelectedServiceUserEthnicity { get; set; }
  public string SelectedServiceUserEthnicityGroup { get; set; }
  public string SelectedStatus { get; set; }
  public List<SelectListItem> SelectedStatusList { get; set; } = new()
  {
    new SelectListItem() { 
      Text = "New",
      Value = ((long)ReferralStatus.New).ToString()},
     new SelectListItem() {
      Text = "RMC Call",
      Value = ((long)ReferralStatus.RmcCall).ToString()}
  };
  public string SelectedStatusReason { get; set; }
  public List<SelectListItem> ServiceUserEthnicityList { get; set; }
  public List<SelectListItem> ServiceUserEthnicityGroupList { get; set; }
  public string Status { get; set; }
  public string StatusReason { get; set; }
  public string Telephone { get; set; }
  public string VulnerableDescription { get; set; }
  public string Ubrn { get; set; }

  private bool IsGpReferral
  {
    get => ReferralSource.TryParseToEnumName(out ReferralSource parsedEnum)
      && parsedEnum == Business.Enums.ReferralSource.GpReferral;
  }
}
