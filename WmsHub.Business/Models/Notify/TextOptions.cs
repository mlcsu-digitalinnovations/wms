using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WmsHub.Business.Enums;
using WmsHub.Common.Extensions;
using WmsHub.Common.Models;

namespace WmsHub.Business.Models.Notify
{
  public class TextOptions : NumberWhiteListOptions, ITextOptions
  {
    public const string SectionKey = "TextSettings";

    public const string TEMPLATE_FAILEDTOCONTACT = "FailedToContact";

    public const string TEMPLATE_GENERAL_FIRST = "GeneralReferralFirst";
    public const string TEMPLATE_GENERAL_SECOND = "GeneralReferralSecond";

    public const string TEMPLATE_GP_FIRST = "GpReferralFirst";
    public const string TEMPLATE_GP_SECOND = "GpReferralSecond";

    public const string TEMPLATE_MSK_FIRST = "MskReferralFirst";
    public const string TEMPLATE_MSK_SECOND = "MskReferralSecond";

    public const string TEMPLATE_NONGP_DECLINED = "NonGpProviderDeclined";
    public const string TEMPLATE_NONGP_REJECTED = "NonGpProviderRejected";
    public const string TEMPLATE_NONGP_TERMINATED = "NonGpProviderTerminated";

    public const string TEMPLATE_NUMBERNOTMONITORED = "NumberNotMonitored";

    public const string TEMPLATE_PHARMACY_FIRST = "PharmacyReferralFirst";
    public const string TEMPLATE_PHARMACY_SECOND = "PharmacyReferralSecond";

    public const string TEMPLATE_SELF_CANCELLEDDUPLICATE =
      "StaffReferralCancelledDuplicate";
    public const string TEMPLATE_SELF_FIRST = "StaffReferralFirstMessage";
    public const string TEMPLATE_SELF_SECOND = "StaffReferralSecondMessage";
    private string _notifyLink;
    private string _generalReferralNotifyLink;

    /// <summary>
    /// WmsHub.GovUkNotify.Api_TextSettings:SmsApiKey
    /// </summary>
    [Required]
    public virtual string SmsApiKey { get; set; }
    /// <summary>
    /// WmsHub.GovUkNotify.Api_TextSettings:SmsSenderId
    /// </summary>
    [Required]
    public virtual string SmsSenderId { get; set; }
    /// <summary>
    /// WmsHub.GovUkNotify.Api_TextSettings:SmsBearerToken
    /// </summary>
    public virtual string SmsBearerToken { get; set; }

    public string TokenPassword { get; set; }
    public bool TokenEnabled { get; set; }

    /// <summary>
    /// WmsHub.GovUkNotify.Api_TextSettings:TokenSecret
    /// </summary>
    ///
    [Required]
    public virtual string TokenSecret { get; set; }
    /// <summary>
    /// appsettings.json NotifyLink
    /// </summary>
    public virtual string NotifyLink
    {
      get => _notifyLink;
      set => _notifyLink = value.EnsureEndsWithForwardSlash();
    }
    /// <summary>
    /// appsettings.json SmsTemplates
    /// </summary>
    public virtual List<SmsTemplate> SmsTemplates { get; set; } =
      new List<SmsTemplate>();
    public virtual Guid GetTemplateIdFor(string templateName)
    {
      if (!SmsTemplates.Any())
        throw new ArgumentOutOfRangeException(
          "Gov.UK Notify Template Id list is empty");

      var found = SmsTemplates.FirstOrDefault(t => t.Name == templateName);
      if (found == null) throw new ArgumentNullException(
        $"Gov.UK Notify Template Id not found using name '{templateName}'");

      return found.Id;
    }

    public virtual Func<Entities.TextMessage, bool>
      SearchPredicate
    { get; set; } =
        t => t.IsActive &&
        t.Sent > DateTime.UtcNow.AddDays(-1) &&
        string.IsNullOrWhiteSpace(t.Outcome);
    public virtual List<string> ValidUsers { get; set; }
    public virtual string Audience { get; set; } = "http://gov.uk";
    public virtual string Issuer { get; set; }

    public virtual DomainAccess Access => DomainAccess.TextMessageApi;
    public virtual string GeneralReferralNotifyLink
    {
      get => _generalReferralNotifyLink;
      set => _generalReferralNotifyLink = value.EnsureEndsWithForwardSlash();
    }
  }
}
